using System;
using System.Collections.Generic;
using Assets.Effects.Base;
using Assets.Targeting;
using Assets.Turns;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public interface IStatusEffectReceiver
    {
        void ApplyStatusEffect(IStatusEffectBase effect);

        void RemoveStatusEffect(IStatusEffectBase effect);

        bool HasStatusEffect(EffectType effectType);

        List<IStatusEffectBase> GetActiveEffects();

        event EventHandler OnEffectsChanged;
    }

    public class StatusEffectReceiver : MonoBehaviour, IStatusEffectReceiver
    {
        private readonly List<IStatusEffectBase> _activeEffects = new();

        public event EventHandler OnEffectsChanged;

        public void ApplyStatusEffect(IStatusEffectBase effect)
        {
            IStatusEffectBase existingEffect = _activeEffects.Find(e => e.EffectType == effect.EffectType);

            if (existingEffect != null)
            {
                existingEffect.StackWith(effect);
                Debug.Log($"Status effect '{effect.EffectType}' stacked on {gameObject.name}");

                if (existingEffect.ExecutionState == TurnExecutionState.Instant)
                {
                    existingEffect.PerformEffect();
                }
            }
            else
            {
                if (gameObject.TryGetComponent(out ITarget target))
                {
                    effect.Apply(target);
                    _activeEffects.Add(effect);
                    Debug.Log($"Status effect '{effect.EffectType}' applied to {gameObject.name}");

                    if (effect.ExecutionState == TurnExecutionState.Instant)
                    {
                        effect.PerformEffect();
                    }
                }
                else
                {
                    Debug.Log($"Status effect '{effect.EffectType}' failed to apply to " +
                        $"{gameObject.name} because of missing ITarget");
                }
            }

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveStatusEffect(IStatusEffectBase effect)
        {
            _activeEffects.Remove(effect);
            Debug.Log($"Status effect '{effect.EffectType}' removed from {gameObject.name}");

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool HasStatusEffect(EffectType effectType)
        {
            return _activeEffects.Exists(e => e.EffectType == effectType);
        }

        public List<IStatusEffectBase> GetActiveEffects()
        {
            return new List<IStatusEffectBase>(_activeEffects);
        }
    }
}
