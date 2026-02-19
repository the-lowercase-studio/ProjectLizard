using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects.Stun
{
    [CreateAssetMenu(fileName = "New Physical Stun Effect", menuName = "Scriptable Objects/Cards/Effects/Physical/Stun Effect")]
    public class PhysicalStunEffectSO : EffectSO
    {
        [field: SerializeField] public int Damage { get; private set; }
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
            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(Damage);
                Debug.Log($"Physical effect dealt {Damage} damage to {context.Target.Name}");
            }
        }

        private void ApplyStunEffect(CardEffectContext context)
        {
            if (Random.value <= StunChance && context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new StunStatusEffect(StunDuration));
                Debug.Log($"Applied burning effect to {context.Target.Name} for {StunDuration} turns");
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
