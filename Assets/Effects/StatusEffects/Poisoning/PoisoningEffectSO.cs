using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Poisoning
{
    [CreateAssetMenu(fileName = "New Acid Poisoning Effect", menuName = "Scriptable Objects/Effects/Acid/Poisoning Effect")]
    public class PoisoningEffectSO : EffectSO
    {
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public int PoisoningDamagePerTurn { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float PoisoningChance { get; private set; }
        [field: SerializeField] public GameObject VisualEffectPrefab { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyPoisoningEffect(context);
            SpawnVisualEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(Damage);
                Debug.Log($"Poisoning effect dealt {Damage} damage to {context.Target.Name}");
            }
        }

        private void ApplyPoisoningEffect(CardEffectContext context)
        {
            if (Random.value <= PoisoningChance && context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new PoisoningStatusEffect(this));
                Debug.Log($"Applied poisoning effect to {context.Target.Name} for {TurnDuration} turns");
            }
        }

        private void SpawnVisualEffect(CardEffectContext context)
        {
            if (VisualEffectPrefab != null)
            {
                Instantiate(VisualEffectPrefab, context.Position, Quaternion.identity);
            }
        }
    }
}
