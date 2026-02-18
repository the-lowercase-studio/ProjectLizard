using Assets.Interfaces.Combat;
using Assets.Turns;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public class BurningStatusEffect : StatusEffectBase
    {
        private readonly int _damagePerTurn;
        private GameObject _visualEffect;

        public BurningStatusEffect(byte turns, int damagePerTurn)
            : base("Burning", turns, TurnExecutionState.OnEnemyTurnStart, EffectType.Burn)
        {
            _damagePerTurn = damagePerTurn;
            EffectValueDisplay = _damagePerTurn.ToString();
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

        private void RemoveVisualEffect()
        {
            if (_visualEffect != null)
            {
                Object.Destroy(_visualEffect);
            }
        }
    }
}
