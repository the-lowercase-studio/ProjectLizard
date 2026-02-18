using Assets.Interfaces.Combat;
using Assets.Turns;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public class BurningStatusEffect : StatusEffectBase
    {
        private int _damagePerTurn;
        private GameObject _visualEffect;

        public BurningStatusEffect(int turns, int damagePerTurn)
            : base(new StatusEffectConfig(
                effectName: "Burning",
                turns: turns,
                executionState: TurnExecutionState.OnEnemyTurnStart,
                canStackValue: true,
                effectType: EffectType.Burn))
        {
            _damagePerTurn = damagePerTurn;
            UpdateEffectValueDisplay();
        }

        protected override void OnApply()
        {
            Debug.Log($"Burning applied! Will last {RemainingTurns} turns, dealing {_damagePerTurn} damage per turn.");
        }

        protected override void ProcessTurnEffect()
        {
            if (Target is IDamageable damageable)
            {
                damageable.TakeDamage(_damagePerTurn);
                Debug.Log($"Burning dealt {_damagePerTurn} damage. {RemainingTurns} turns remaining.");
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
                _damagePerTurn += burningEffect._damagePerTurn;
                Debug.Log($"Burning damage stacked! Now dealing {_damagePerTurn} damage per turn for {RemainingTurns} turns.");
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
