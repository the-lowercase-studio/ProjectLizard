using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Bleeding
{
    [CreateAssetMenu(fileName = "New Physic Bleeding", menuName = "Scriptable Objects/Effects/Physic/Bleeding")]
    public class BleedingEffectSO : EffectSO
    {
        [field: SerializeField] public int InitialDamage { get; private set; }
        [field: SerializeField] public int BleedingDamagePerTurn { get; private set; }
        [field: SerializeField] public float AcidDamageMultiplier { get; private set; }
        [field: SerializeField] public float PhysicDamageMultiplier { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyBleedingEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(InitialDamage);
                Debug.Log($"Fire effect dealt {InitialDamage} damage to {context.Target.Name}");
            }
        }

        private void ApplyBleedingEffect(CardEffectContext context)
        {
            if (context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new BleedingStatusEffect(this));
                Debug.Log($"Applied burning effect to {context.Target.Name} for {TurnDuration} turns");
            }
        }
    }
}
