using Assets.Effects.Base;
using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.UI
{
    public interface IAppliedEffectsPresenter : IInitializableByConfig<IStatusEffectReceiver>
    {
        void UpdateEffectsDisplay();
    }

    public class AppliedEffectsPresenter : MonoBehaviour, IAppliedEffectsPresenter
    {
        [SerializeField] private AppliedEffectPresenter _effectPresenterPrefab;
        [SerializeField] private EffectTypeSpriteMappingSO _effectTypeMapping;

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
                    CreateEffectPresenter(effect);
                }
            }
        }

        private void CreateEffectPresenter(IStatusEffect effect)
        {
            Sprite effectSprite = _effectTypeMapping.GetSpriteForEffectType(effect.EffectType);

            AppliedEffectPresenter presenter = Instantiate(_effectPresenterPrefab, transform);
            presenter.Initialize(new AppliedEffectPresenterConfig(effect, effectSprite));
            _activePresenters[effect.EffectType] = presenter;
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
