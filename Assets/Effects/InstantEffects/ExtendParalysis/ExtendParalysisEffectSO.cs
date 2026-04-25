using Assets.Effects.Base;
using Assets.Effects.StatusEffects;
using Assets.Targeting;
using Assets.Turns;
using System.Linq;
using UnityEngine;

namespace Assets.Effects.InstantEffects.ExtendParalysis
{
    [CreateAssetMenu(fileName = "New Extend Paralysis Effect", menuName = "Scriptable Objects/Effects/Electric/Extend Paralysis Effect")]
    public class ExtendParalysisEffectSO : EffectSO
    {
        public override void Execute(CardEffectContext context)
        {
            if (context.Target?.StatusEffectReceiver != null)
            {
                var activeEffects = context.Target.StatusEffectReceiver.GetActiveEffects();
                var paralysisEffect = activeEffects.FirstOrDefault(e => e.EffectType == EffectType.Paralysis);

                if (paralysisEffect != null)
                {
                    context.Target.StatusEffectReceiver.ApplyStatusEffect(new ParalysisExtension(TurnDuration));
                    Debug.Log($"Extended Paralysis on {context.Target.Name} by {TurnDuration} turn(s).");
                }
            }
        }
    }
}
