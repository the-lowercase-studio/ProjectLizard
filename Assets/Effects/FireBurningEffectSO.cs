using Assets.Effects.Base;
using Assets.Effects.StatusEffects;
using UnityEngine;

namespace Assets.Effects
{
    [CreateAssetMenu(fileName = "New Fire Burning Effect", menuName = "Scriptable Objects/Cards/Effects/Fire/Burning Effect")]
    public class FireBurningEffectSO : EffectSO
    {
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float BurningChance { get; private set; }
        [field: SerializeField] public byte BurningDuration { get; private set; }
        [field: SerializeField] public int BurnDamagePerTurn { get; private set; }
        [field: SerializeField] public GameObject VisualEffectPrefab { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            ApplyDirectDamage(context);
            ApplyBurningEffect(context);
            SpawnVisualEffect(context);
        }

        private void ApplyDirectDamage(CardEffectContext context)
        {
            Debug.Log($"START APPLY DAMAGE {context.Target}");

            if (context.Target?.Damageable != null)
            {
                context.Target.Damageable.TakeDamage(Damage);
                Debug.Log($"Fire effect dealt {Damage} damage to {context.Target.Name}");
            }
        }

        private void ApplyBurningEffect(CardEffectContext context)
        {
            if (Random.value <= BurningChance && context.Target?.StatusEffectReceiver != null)
            {
                context.Target.StatusEffectReceiver.ApplyStatusEffect(new BurningStatusEffect(BurningDuration, BurnDamagePerTurn));
                Debug.Log($"Applied burning effect to {context.Target.Name} for {BurningDuration} turns");
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
