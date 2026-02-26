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
            if (target is IDamageable damageable)
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
            ITarget spreadTarget = _targetsProvider.GetClosestByDirection(
                target, CustomTypes.HorizontalDirection.Right);

            if (!CanSpreadOnTarget(spreadTarget))
            {
                spreadTarget = _targetsProvider.GetClosestByDirection(
                    target, CustomTypes.HorizontalDirection.Left);

                if (!CanSpreadOnTarget(spreadTarget))
                {
                    return;
                }
            }

            if (spreadTarget.StatusEffectReceiver == null)
            {
                Debug.LogWarning($"Spread target {spreadTarget} has no StatusEffectReceiver.");
                return;
            }

            spreadTarget.StatusEffectReceiver.ApplyStatusEffect(
                new BurningStatusEffect(effectData as BurningEffectSO, _targetsProvider));
        }

        private bool CanSpreadOnTarget(ITarget target)
        {
            return target?.StatusEffectReceiver.HasStatusEffect(EffectType) == false;
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
