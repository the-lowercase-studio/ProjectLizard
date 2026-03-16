using Assets.Interfaces.Combat;
using Assets.Turns;
using UnityEngine;
using Assets.Effects.Base;

namespace Assets.Effects.StatusEffects.Paralysis
{
    public class ParalysisStatusEffect : StatusEffectBase
    {
        private GameObject _visualEffect;

        public ParalysisStatusEffect(ParalysisEffectSO effectSO)
            : base(effectSO)
        {
        }

        protected override void OnApply()
        {
            Debug.Log($"Paralysis applied! Target cannot act for {RemainingTurns} turns.");

            if (target is IParalyzable controllable)
            {
                controllable.ApplyParalysis();
            }
        }

        protected override void ProcessTurnEffect()
        {
            Debug.Log($"Still stunned. {RemainingTurns} turns remaining.");
        }

        protected override void OnRemove()
        {
            Debug.Log("Paralysis effect removed.");

            if (target is IParalyzable controllable)
            {
                controllable.RemoveParalysis();
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
