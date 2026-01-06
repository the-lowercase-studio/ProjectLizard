using Assets.Interfaces.Combat;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public class StunStatusEffect : StatusEffectBase
    {
        private GameObject _visualEffect;

        public StunStatusEffect(byte turns) : base("Stunned", turns)
        {
        }

        protected override void OnApply()
        {
            Debug.Log($"Stun applied! Target cannot act for {RemainingTurns} turns.");

            if (Target is IStunnable controllable)
            {
                controllable.ApplyStun(false);
            }

            SpawnVisualEffect();
        }

        protected override void ProcessTurnEffect()
        {
            Debug.Log($"Still stunned. {RemainingTurns} turns remaining.");
        }

        protected override void OnRemove()
        {
            Debug.Log("Stun effect removed.");

            if (Target is IStunnable controllable)
            {
                controllable.ApplyStun(true);
            }

            RemoveVisualEffect();
        }

        private void SpawnVisualEffect()
        {
            // TODO: Load and instantiate stun VFX prefab on target
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
