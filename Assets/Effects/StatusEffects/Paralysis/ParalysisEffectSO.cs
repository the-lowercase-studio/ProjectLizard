using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Paralysis
{
    [CreateAssetMenu(fileName = "New Electric Paralysis", menuName = "Scriptable Objects/Effects/Electric/Paralysis")]
    public class ParalysisEffectSO : EffectSO
    {
        [field: SerializeField] public int InitialDamage { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDamage(context);
            ApplyParalysisEffect(context);
        }

        private void ApplyDamage(CardEffectContext context)
        {
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(InitialDamage);
                Debug.Log($"Paralysis effect dealt {InitialDamage} damage to {context.Target.Name}");
            }
        }

        private void ApplyParalysisEffect(CardEffectContext context)
        {
            if (context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new ParalysisStatusEffect(this));
                Debug.Log($"Applied paralysis effect to {context.Target.Name} for {TurnDuration} turns");
            }
        }
    }
}
