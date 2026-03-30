using Assets.Effects.Base;
using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.UI
{
    public interface IEffectsPresenter : IInitializableByConfig<IStatusEffectReceiver>
    {
        void UpdateEffectsDisplay();
    }

    public class EffectsPresenter : MonoBehaviour, IEffectsPresenter
    {
        [SerializeField] private EffectTypeSpriteMappingSO _effectTypeMapping;
        [SerializeField] private AppliedEffectPresenter _effectPresenterPrefab;
        [SerializeField] private InitialEffectPresenter _initialEffectPresenterPrefab;
        [SerializeField] private Transform _initialEffectPresenterParent;

        private IStatusEffectReceiver _targetReceiver;
        private Dictionary<EffectType, AppliedEffectPresenter> _activePresenters = new Dictionary<EffectType, AppliedEffectPresenter>();

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
            List<IStatusEffect> activeEffects = _targetReceiver.GetActiveEffects();

            List<EffectType> effectsToRemove = new List<EffectType>();
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

            foreach (IStatusEffect effect in activeEffects)
            {
                if (effect.RemainingTurns <= 0) continue;

                if (_activePresenters.ContainsKey(effect.EffectType))
                {
                    _activePresenters[effect.EffectType].UpdateDisplay();
                }
                else
                {
                    Sprite effectSprite = _effectTypeMapping.GetSpriteForEffectType(effect.EffectType);
                    CreateAppliedEffectPresenter(effect.EffectType, effect, effectSprite);
                    CreateInitialEffectPresenter(effect.EffectType, effectSprite);
                }
            }
        }

        private void CreateAppliedEffectPresenter(EffectType effectType, IStatusEffect statusEffect, Sprite effectSprite)
        {
            AppliedEffectPresenter presenter = Instantiate(_effectPresenterPrefab, transform);
            presenter.Initialize(new AppliedEffectPresenterConfig(statusEffect, effectSprite));
            _activePresenters[effectType] = presenter;
        }

        private void CreateInitialEffectPresenter(EffectType effectType, Sprite effectSprite)
        {
            var initialEffectAnimator = _effectTypeMapping.GetInitialEffectAnimatorForEffectType(effectType);
            if (initialEffectAnimator == null)
            {
                return;
            }

            Transform parent = _initialEffectPresenterParent != null ? _initialEffectPresenterParent : transform;
            InitialEffectPresenter presenter = Instantiate(_initialEffectPresenterPrefab, parent);
            presenter.Initialize(new InitialEffectPresenterConfig(effectType, effectSprite, initialEffectAnimator));
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
