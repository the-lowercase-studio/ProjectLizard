using Assets.Interfaces.Combat;
using UnityEngine;
using Assets.Targeting;

namespace Assets.Effects.StatusEffects.Burning
{
    public class BurningStatusEffect : StatusEffectBase
    {
        public int DamagePerTurn { get; private set; }

        private readonly ITargetsProvider _targetsProvider;
        private readonly float _spreadChance;
        private readonly GameObject _visualEffect;

        public BurningStatusEffect(BurningEffectSO effectSO, ITargetsProvider targetsProvider)
            : base(effectSO)
        {
            _targetsProvider = targetsProvider;
            DamagePerTurn = effectSO.BurningDamagePerTurn;
            _spreadChance = effectSO.BurningSpreadChance;

            UpdateEffectValueDisplay();
        }

        protected override void OnApply()
        {
            Debug.Log($"Burning applied! Will last {RemainingTurns} turns, " +
                $"dealing {DamagePerTurn} damage per turn.");
        }

        protected override void ProcessTurnEffect()
        {
            if (Target is IDamageable damageable)
            {
                damageable.TakeDamage(DamagePerTurn);
                Debug.Log($"Burning dealt {DamagePerTurn} damage. {RemainingTurns} turns remaining.");

                if (Random.value <= _spreadChance)
                {
                    SpreadEffect();
                }
            }
        }

        protected override void OnRemove()
        {
            Debug.Log("Burning effect removed.");
            RemoveVisualEffect();
        }

        protected override void StackValue(IStatusEffect other)
        {
            if (other is BurningStatusEffect burningEffect)
            {
                DamagePerTurn += burningEffect.DamagePerTurn;
                Debug.Log($"Burning damage stacked! Now dealing {DamagePerTurn} damage " +
                    $"per turn for {RemainingTurns} turns.");
            }
        }

        protected override void UpdateEffectValueDisplay()
        {
            EffectValueDisplay = DamagePerTurn.ToString();
        }

        private void SpreadEffect()
        {
            ITarget spreadTarget = _targetsProvider.GetClosest(Target);

            if (spreadTarget == null || spreadTarget == Target)
            {
                return;
            }

            if (spreadTarget.StatusEffectReceiver == null)
            {
                Debug.LogWarning($"Spread target {spreadTarget} has no StatusEffectReceiver.");
                return;
            }

            spreadTarget.StatusEffectReceiver.ApplyStatusEffect(
                new BurningStatusEffect(EffectData as BurningEffectSO, _targetsProvider));
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
