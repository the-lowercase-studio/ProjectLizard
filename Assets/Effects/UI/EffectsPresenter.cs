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
        private Dictionary<string, EffectPresenter> _activePresenters = new Dictionary<string, EffectPresenter>();

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

            List<string> effectsToRemove = new List<string>();
            foreach (var presenter in _activePresenters)
            {
                bool stillActive = activeEffects.Exists(e => e.EffectName == presenter.Key);
                if (!stillActive || presenter.Value.IsEffectExpired())
                {
                    effectsToRemove.Add(presenter.Key);
                }
            }

            foreach (string effectName in effectsToRemove)
            {
                RemoveEffectPresenter(effectName);
            }

            foreach (IStatusEffect effect in activeEffects)
            {
                if (effect.RemainingTurns <= 0) continue;

                if (_activePresenters.ContainsKey(effect.EffectName))
                {
                    _activePresenters[effect.EffectName].UpdateDisplay();
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
            _activePresenters[effect.EffectName] = presenter;
        }

        private void RemoveEffectPresenter(string effectName)
        {
            if (_activePresenters.TryGetValue(effectName, out EffectPresenter presenter))
            {
                Destroy(presenter.gameObject);
                _activePresenters.Remove(effectName);
            }
        }
    }
}
