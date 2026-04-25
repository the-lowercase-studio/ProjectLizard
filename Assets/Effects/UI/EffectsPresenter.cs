using System.Collections.Generic;
using Assets.Effects.Base;
using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using UnityEngine;

namespace Assets.Effects.UI
{
    public interface IEffectsPresenter : IInitializableByConfig<IStatusEffectReceiver>
    {
        void UpdateEffectsDisplay();
    }

    public class EffectsPresenter : MonoBehaviour, IEffectsPresenter
    {
        [SerializeField] private AppliedEffectPresenter _effectPresenterPrefab;
        [SerializeField] private InitialEffectPresenter _initialEffectPresenterPrefab;
        [SerializeField] private Transform _initialEffectPresenterParent;

        private IStatusEffectReceiver _targetReceiver;
        private readonly Dictionary<EffectType, AppliedEffectPresenter> _activePresenters = new();

        private void OnDestroy()
        {
            foreach (var presenter in _activePresenters.Values)
            {
                if (presenter != null)
                {
                    Destroy(presenter.gameObject);
                }
            }

            _activePresenters.Clear();
        }

        public void Initialize(IStatusEffectReceiver receiver)
        {
            _targetReceiver = receiver;
        }

        public void UpdateEffectsDisplay()
        {
            List<IStatusEffectBase> activeEffects = _targetReceiver.GetActiveEffects();

            List<EffectType> effectsToRemove = new();
            foreach (var presenter in _activePresenters)
            {
                bool stillActive = activeEffects.Exists(e => e.EffectType == presenter.Key);
                if (!stillActive || presenter.Value.IsEffectExpired())
                {
                    effectsToRemove.Add(presenter.Key);
                }
            }

            foreach (EffectType effectType in effectsToRemove)
            {
                RemoveEffectPresenter(effectType);
            }

            foreach (IStatusEffectBase effect in activeEffects)
            {
                if (effect.RemainingTurns <= 0) continue;

                if (_activePresenters.ContainsKey(effect.EffectType))
                {
                    _activePresenters[effect.EffectType].UpdateDisplay();
                }
                else
                {
                    Sprite effectSprite = effect.EffectData?.Sprite;
                    CreateAppliedEffectPresenter(effect.EffectType, effect, effectSprite);
                    CreateInitialEffectPresenter(effect);
                }
            }
        }

        private void CreateAppliedEffectPresenter(EffectType effectType, IStatusEffectBase statusEffect, Sprite effectSprite)
        {
            AppliedEffectPresenter presenter = Instantiate(_effectPresenterPrefab, transform);
            presenter.Initialize(new AppliedEffectPresenterConfig(statusEffect, effectSprite));
            _activePresenters[effectType] = presenter;
        }

        private void CreateInitialEffectPresenter(IStatusEffectBase effect)
        {
            var initialEffectAnimator = effect.EffectData?.InitialEffectAnimator;
            if (initialEffectAnimator == null)
            {
                return;
            }

            InitialEffectPresenter presenter = Instantiate(_initialEffectPresenterPrefab, _initialEffectPresenterParent);
            presenter.Initialize(new InitialEffectPresenterConfig(initialEffectAnimator));
        }

        private void RemoveEffectPresenter(EffectType effectType)
        {
            if (_activePresenters.TryGetValue(effectType, out AppliedEffectPresenter presenter))
            {
                Destroy(presenter.gameObject);
                _activePresenters.Remove(effectType);
            }
        }
    }
}
