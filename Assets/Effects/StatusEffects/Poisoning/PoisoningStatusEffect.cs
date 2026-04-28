using Assets.ElementalSystem;
using Assets.Interfaces.Combat;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Poisoning
{
    public class PoisoningStatusEffect : StatusEffectBase, IIncomingDamageModifier
    {
        private int _damagePerTurn;
        private GameObject _visualEffect;
        private readonly PoisoningEffectSO _effectSO;

        public PoisoningStatusEffect(PoisoningEffectSO effectSO)
            : base(effectSO)
        {
            _effectSO = effectSO;
            _damagePerTurn = effectSO.PoisoningDamagePerTurn;
            UpdateEffectValueDisplay();
        }

        public int ModifyIncomingDamage(int incomingDamage, Elements damageElement)
        {
            float scalingFactor = _effectSO.GetDamageScalingFactor(damageElement);
            int scaledDamage = Mathf.CeilToInt(incomingDamage * scalingFactor);

            return Mathf.Max(scaledDamage, incomingDamage);
        }

        protected override void OnApply()
        {
            Debug.Log($"Poisoning applied! Will last {RemainingTurns} turns, dealing {_damagePerTurn} damage per turn.");
        }

        protected override void ProcessTurnEffect()
        {
            if (target is IDamageable damageable)
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

        protected override void StackValue(IStatusEffectBase other)
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
