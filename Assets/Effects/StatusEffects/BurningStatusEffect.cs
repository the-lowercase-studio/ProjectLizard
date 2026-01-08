using Assets.Interfaces.Combat;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public class BurningStatusEffect : StatusEffectBase
    {
        private readonly int damagePerTurn;
        private GameObject visualEffect;

        public BurningStatusEffect(byte turns, int damagePerTurn) : base("Burning", turns)
        {
            this.damagePerTurn = damagePerTurn;
        }

        protected override void OnApply()
        {
            Debug.Log($"Burning applied! Will last {RemainingTurns} turns, dealing {damagePerTurn} damage per turn.");

            SpawnVisualEffect();
        }

        protected override void ProcessTurnEffect()
        {
            if (Target is IDamageable damageable)
            {
                damageable.TakeDamage(damagePerTurn);
                Debug.Log($"Burning dealt {damagePerTurn} damage. {RemainingTurns} turns remaining.");
            }
        }

        protected override void OnRemove()
        {
            Debug.Log("Burning effect removed.");
            RemoveVisualEffect();
        }

        private void SpawnVisualEffect()
        {
            // TODO: Load and instantiate burning VFX prefab on target
            // visualEffect = Object.Instantiate(burningVFXPrefab, target.transform);
        }

        private void RemoveVisualEffect()
        {
            if (visualEffect != null)
            {
                Object.Destroy(visualEffect);
            }
        }
    }
}
