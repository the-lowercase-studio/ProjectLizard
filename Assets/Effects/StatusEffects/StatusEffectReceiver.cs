using Assets.Effects.Base;
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

        bool HasStatusEffect(EffectType effectType);

        List<IStatusEffect> GetActiveEffects();

        event EventHandler OnEffectsChanged;
    }

    public class StatusEffectReceiver : MonoBehaviour, IStatusEffectReceiver
    {
        private List<IStatusEffect> _activeEffects = new List<IStatusEffect>();

        public event EventHandler OnEffectsChanged;

        public void ApplyStatusEffect(IStatusEffect effect)
        {
            IStatusEffect existingEffect = _activeEffects.Find(e => e.EffectType == effect.EffectType);

            if (existingEffect != null)
            {
                existingEffect.StackWith(effect);
                Debug.Log($"Status effect '{effect.EffectType}' stacked on {gameObject.name}");
            }
            else
            {
                if (gameObject.TryGetComponent(out ITarget target))
                {
                    effect.Apply(target);
                    _activeEffects.Add(effect);
                    Debug.Log($"Status effect '{effect.EffectType}' applied to {gameObject.name}");
                }
                else
                {
                    Debug.Log($"Status effect '{effect.EffectType}' failed to apply to " +
                        $"{gameObject.name} because of missing ITarget");
                }
            }

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveStatusEffect(IStatusEffect effect)
        {
            _activeEffects.Remove(effect);
            Debug.Log($"Status effect '{effect.EffectType}' removed from {gameObject.name}");

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool HasStatusEffect(EffectType effectType)
        {
            return _activeEffects.Exists(e => e.EffectType == effectType);
        }

        public List<IStatusEffect> GetActiveEffects()
        {
            return new List<IStatusEffect>(_activeEffects);
        }
    }
}
