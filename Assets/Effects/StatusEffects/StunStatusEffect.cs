using Assets.Interfaces.Combat;
using Assets.Turns;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    public class StunStatusEffect : StatusEffectBase
    {
        private GameObject _visualEffect;

        public StunStatusEffect(int turns)
            : base(new StatusEffectConfig(
                effectName: "Stunned",
                turns: turns,
                executionState: TurnExecutionState.OnPlayerTurnStart,
                canStackValue: false,
                effectType: EffectType.Stun))
        {
        }

        protected override void OnApply()
        {
            Debug.Log($"Stun applied! Target cannot act for {RemainingTurns} turns.");

            if (Target is IStunnable controllable)
            {
                controllable.ApplyStun(false);
            }
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

        private void RemoveVisualEffect()
        {
            if (_visualEffect != null)
            {
                Object.Destroy(_visualEffect);
            }
        }
    }
}
