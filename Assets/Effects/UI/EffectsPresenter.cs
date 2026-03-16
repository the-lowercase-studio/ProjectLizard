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
        [SerializeField] private EffectPresenter _effectPresenterPrefab;
        [SerializeField] private EffectTypeSpriteMappingSO _effectTypeMapping;

        private IStatusEffectReceiver _targetReceiver;
        private Dictionary<EffectType, EffectPresenter> _activePresenters = new Dictionary<EffectType, EffectPresenter>();

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

            EffectPresenter presenter = Instantiate(_effectPresenterPrefab, transform);
            presenter.Initialize(new EffectPresenterConfig(effect, effectSprite));
            _activePresenters[effect.EffectType] = presenter;
        }

        private void RemoveEffectPresenter(EffectType effectType)
        {
            if (_activePresenters.TryGetValue(effectType, out EffectPresenter presenter))
            {
                Destroy(presenter.gameObject);
                _activePresenters.Remove(effectType);
            }
        }
    }
}
