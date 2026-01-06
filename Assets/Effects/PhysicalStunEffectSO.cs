using Assets.Effects.StatusEffects;
using Assets.Interfaces.Combat;
using UnityEngine;

namespace Assets.Effects
{
    [CreateAssetMenu(fileName = "New Physical Stun Effect", menuName = "Scriptable Objects/Cards/Effects/Physical/Stun Effect")]
    public class PhysicalStunEffectSO : EffectSO
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float StunChance { get; private set; }
        [field: SerializeField] public byte StunDuration { get; private set; }
        [field: SerializeField] public GameObject VisualEffectPrefab { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDamage(context);
            ApplyStunEffect(context);
            SpawnVisualEffect(context);
        }

        private void ApplyDamage(CardEffectContext context)
        {
            if (context.Target != null && context.Target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(Damage);
                Debug.Log($"Physical effect dealt {Damage} damage to {context.Target.name}");
            }
        }

        private void ApplyStunEffect(CardEffectContext context)
        {
            if (Random.value <= StunChance && context.Target != null && context.Target.TryGetComponent(out IStatusEffectReceiver statusReceiver))
            {
                statusReceiver.ApplyStatusEffect(new StunStatusEffect(StunDuration));
                Debug.Log($"Applied burning effect to {context.Target.name} for {StunDuration} turns");
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
