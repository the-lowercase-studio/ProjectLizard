using Assets.Effects.StatusEffects;
using Assets.Interfaces.Combat;
using UnityEngine;

namespace Assets.Effects
{
    [CreateAssetMenu(fileName = "New Fire Burning Effect", menuName = "Scriptable Objects/Cards/Effects/Fire/Burning Effect")]
    public class FireBurningEffectSO : EffectSO
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float BurningChance { get; private set; }
        [field: SerializeField] public byte BurningDuration { get; private set; }
        [field: SerializeField] public float BurnDamagePerTurn { get; private set; }
        [field: SerializeField] public GameObject VisualEffectPrefab { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyBurningEffect(context);
            SpawnVisualEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            if (context.Target != null && context.Target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(Damage);
                Debug.Log($"Fire effect dealt {Damage} damage to {context.Target.name}");
            }
        }

        private void ApplyBurningEffect(CardEffectContext context)
        {
            if (Random.value <= BurningChance && context.Target != null && context.Target.TryGetComponent(out IStatusEffectReceiver statusReceiver))
            {
                statusReceiver.ApplyStatusEffect(new BurningStatusEffect(BurningDuration, BurnDamagePerTurn));
                Debug.Log($"Applied burning effect to {context.Target.name} for {BurningDuration} turns");
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
