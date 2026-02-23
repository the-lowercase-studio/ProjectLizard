using Assets.Interfaces.Combat;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Poisoning
{
    public class PoisoningStatusEffect : StatusEffectBase
    {
        private int _damagePerTurn;
        private GameObject _visualEffect;

        public PoisoningStatusEffect(PoisoningEffectSO effectSO)
            : base(effectSO)
        {
            _damagePerTurn = effectSO.PoisoningDamagePerTurn;
            UpdateEffectValueDisplay();
        }

        protected override void OnApply()
        {
            Debug.Log($"Poisoning applied! Will last {RemainingTurns} turns, dealing {_damagePerTurn} damage per turn.");
        }

        protected override void ProcessTurnEffect()
        {
            if (Target is IDamageable damageable)
            {
                damageable.TakeDamage(_damagePerTurn);
                Debug.Log($"Poisoning dealt {_damagePerTurn} damage. {RemainingTurns} turns remaining.");
            }
        }

        protected override void OnRemove()
        {
            Debug.Log("Poisoning effect removed.");
            RemoveVisualEffect();
        }

        protected override void StackValue(IStatusEffect other)
        {
            if (other is PoisoningStatusEffect burningEffect)
            {
                _damagePerTurn += burningEffect._damagePerTurn;
                Debug.Log($"Poisoning damage stacked! Now dealing {_damagePerTurn} damage per turn for {RemainingTurns} turns.");
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
