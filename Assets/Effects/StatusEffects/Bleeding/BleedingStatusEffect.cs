using Assets.Interfaces.Combat;
using Assets.Turns;
using UnityEngine;
using Assets.Effects.Base;

namespace Assets.Effects.StatusEffects.Bleeding
{
    public class BleedingStatusEffect : StatusEffectBase
    {
        private int _damagePerTurn;
        private GameObject _visualEffect;

        public BleedingStatusEffect(BleedingEffectSO effectSO)
            : base(effectSO)
        {
            _damagePerTurn = effectSO.BleedingDamagePerTurn;
            UpdateEffectValueDisplay();
        }

        protected override void OnApply()
        {
            Debug.Log($"Bleeding applied! Will last {RemainingTurns} turns, dealing {_damagePerTurn} damage per turn.");
        }

        protected override void ProcessTurnEffect()
        {
            if (Target is IDamageable damageable)
            {
                damageable.TakeDamage(_damagePerTurn);
                Debug.Log($"Bleeding dealt {_damagePerTurn} damage. {RemainingTurns} turns remaining.");
            }
        }

        protected override void OnRemove()
        {
            Debug.Log("Bleeding effect removed.");
            RemoveVisualEffect();
        }

        protected override void StackValue(IStatusEffect other)
        {
            if (other is BleedingStatusEffect bleedingEffect)
            {
                _damagePerTurn += bleedingEffect._damagePerTurn;
                Debug.Log($"Bleeding damage stacked! Now dealing {_damagePerTurn} damage per turn for {RemainingTurns} turns.");
            }
        }

        protected override void UpdateEffectValueDisplay()
        {
            EffectValueDisplay = _damagePerTurn.ToString();
        }

        private void RemoveVisualEffect()
        {
            if (_visualEffect != null)
            {
                Object.Destroy(_visualEffect);
            }
        }
    }
}
