using Assets.Effects.Base;
using Assets.ElementalSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Poisoning
{
    [CreateAssetMenu(fileName = "New Acid Poisoning", menuName = "Scriptable Objects/Effects/Acid/Poisoning")]
    public class PoisoningEffectSO : EffectSO
    {
        [field: SerializeField] public int InitialDamage { get; private set; }
        [field: SerializeField] public int PoisoningDamagePerTurn { get; private set; }
        [field: SerializeField] public List<Elements> CompatibleElements { get; private set; } = new();
        [field: SerializeField, Min(1f)] public float CompatibleElementsDamageScalingFactor { get; private set; } = 1.25f;
        [field: SerializeField, Min(1f)] public float IncompatibleElementsDamageScalingFactor { get; private set; } = 1.1f;

        public float GetDamageScalingFactor(Elements damageElement)
        {
            return IsCompatibleElement(damageElement)
                ? CompatibleElementsDamageScalingFactor
                : IncompatibleElementsDamageScalingFactor;
        }

        public bool IsCompatibleElement(Elements damageElement)
        {
            return CompatibleElements != null && CompatibleElements.Contains(damageElement);
        }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyPoisoningEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(InitialDamage);
                Debug.Log($"Poisoning effect dealt {InitialDamage} damage to {context.Target.Name}");
            }
        }

        private void ApplyPoisoningEffect(CardEffectContext context)
        {
            if (context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new PoisoningStatusEffect(this));
                Debug.Log($"Applied poisoning effect to {context.Target.Name} for {TurnDuration} turns");
            }
        }
    }
}
