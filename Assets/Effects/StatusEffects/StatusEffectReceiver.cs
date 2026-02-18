using Assets.Targeting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public interface IStatusEffectReceiver
    {
        void ApplyStatusEffect(IStatusEffect effect);

        void RemoveStatusEffect(IStatusEffect effect);

        bool HasStatusEffect(string effectName);

        List<IStatusEffect> GetActiveEffects();

        event EventHandler OnEffectsChanged;
    }

    public class StatusEffectReceiver : MonoBehaviour, IStatusEffectReceiver
    {
        private List<IStatusEffect> _activeEffects = new List<IStatusEffect>();

        public event EventHandler OnEffectsChanged;

        public void ApplyStatusEffect(IStatusEffect effect)
        {
            // Check if effect already exists and refresh duration instead of stacking
            IStatusEffect existingEffect = _activeEffects.Find(e => e.EffectName == effect.EffectName);

            if (existingEffect != null)
            {
                existingEffect.Remove();
                _activeEffects.Remove(existingEffect);
            }

            if (gameObject.TryGetComponent(out ITarget target))
            {
                effect.Apply(target);

                _activeEffects.Add(effect);

                Debug.Log($"Status effect '{effect.EffectName}' applied to {gameObject.name}");

                OnEffectsChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Debug.Log($"Status effect '{effect.EffectName}' failed to applied to " +
                    $"{gameObject.name} becouse of missing ITarget");
            }
        }

        public void RemoveStatusEffect(IStatusEffect effect)
        {
            _activeEffects.Remove(effect);
            Debug.Log($"Status effect '{effect.EffectName}' removed from {gameObject.name}");

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool HasStatusEffect(string effectName)
        {
            return _activeEffects.Exists(e => e.EffectName == effectName);
        }

        public List<IStatusEffect> GetActiveEffects()
        {
            return new List<IStatusEffect>(_activeEffects);
        }
    }
}
