using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Burning
{
    [CreateAssetMenu(fileName = "New Fire Burning Effect", menuName = "Scriptable Objects/Effects/Fire/Burning Effect")]
    public class BurningEffectSO : EffectSO
    {
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public int BurningDamagePerTurn { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float BurningChance { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float BurningSpreadChance { get; private set; }
        [field: SerializeField] public GameObject VisualEffectPrefab { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyBurningEffect(context);
            SpawnVisualEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(Damage);
                Debug.Log($"Burning effect dealt {Damage} damage to {context.Target.Name}");
            }
        }

        private void ApplyBurningEffect(CardEffectContext context)
        {
            if (Random.value <= BurningChance && context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new BurningStatusEffect(this, context.TargetsProvider));
                Debug.Log($"Applied burning effect to {context.Target.Name} for {TurnDuration} turns");
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
