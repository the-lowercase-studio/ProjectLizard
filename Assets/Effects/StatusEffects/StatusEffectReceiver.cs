using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public interface IStatusEffectReceiver
    {
        void ApplyStatusEffect(IStatusEffect effect);

        void RemoveStatusEffect(IStatusEffect effect);

        void ProcessStatusEffectsOnTurnStart();

        void ProcessStatusEffectsOnTurnEnd();

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

            effect.Apply(this);
            _activeEffects.Add(effect);

            Debug.Log($"Status effect '{effect.EffectName}' applied to {gameObject.name}");

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveStatusEffect(IStatusEffect effect)
        {
            _activeEffects.Remove(effect);
            Debug.Log($"Status effect '{effect.EffectName}' removed from {gameObject.name}");

            OnEffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ProcessStatusEffectsOnTurnStart()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].OnTurnStart();
            }
        }

        public void ProcessStatusEffectsOnTurnEnd()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].OnTurnEnd();
            }
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
