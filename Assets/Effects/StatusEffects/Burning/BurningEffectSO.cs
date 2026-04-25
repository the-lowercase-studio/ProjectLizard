using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Burning
{
    [CreateAssetMenu(fileName = "New Fire Burning", menuName = "Scriptable Objects/Effects/Fire/Burning")]
    public class BurningEffectSO : EffectSO
    {
        [field: SerializeField] public int InitialDamage { get; private set; }
        [field: SerializeField] public int BurningDamagePerTurn { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float BurningSpreadChance { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyBurningEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(InitialDamage);
                Debug.Log($"Burning effect dealt {InitialDamage} damage to {context.Target.Name}");
            }
        }

        private void ApplyBurningEffect(CardEffectContext context)
        {
            if (context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new BurningStatusEffect(this, context.TargetsProvider));
                Debug.Log($"Applied burning effect to {context.Target.Name} for {TurnDuration} turns");
            }
        }
    }
}
