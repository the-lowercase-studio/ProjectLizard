using Assets.Effects.Base;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.InstantEffects.ConditionalDamage
{
    [CreateAssetMenu(fileName = "New Conditional Damage", menuName = "Scriptable Objects/Effects/Custom/Conditional Damage")]
    public class ConditionalDamageMultiplierEffectSO : EffectSO
    {
        [field: Header("Condition Details")]
        [field: SerializeField, Tooltip("List of effect types. If the target has ANY of these, the damage is multiplied.")]
        public List<EffectType> RequiredStatusEffects { get; private set; }

        [field: SerializeField, Tooltip("The multiplier applied to the attack's step damage if the condition is met.")]
        public float DamageMultiplier { get; private set; } = 1.0f;

        public override void Execute(CardEffectContext context)
        {
            if (context.Target == null || context.Target.Damageable == null)
            {
                return;
            }

            int stepDamage = context.StepDamage;
            if (stepDamage <= 0)
            {
                return; // Nothing to multiply or add
            }

            bool conditionMet = false;

            if (context.Target.StatusEffectReceiver != null && RequiredStatusEffects != null && RequiredStatusEffects.Count > 0)
            {
                var activeEffects = context.Target.StatusEffectReceiver.GetActiveEffects();
                foreach (var activeEffect in activeEffects)
                {
                    if (RequiredStatusEffects.Contains(activeEffect.EffectType))
                    {
                        conditionMet = true;
                        break;
                    }
                }
            }

            if (conditionMet)
            {
                // Option A: Deal only the bonus damage (since base attack already dealt stepDamage)
                int bonusDamage = Mathf.RoundToInt(stepDamage * DamageMultiplier) - stepDamage;
                if (bonusDamage > 0)
                {
                    context.Target.Damageable.TakeDamage(bonusDamage);
                    Debug.Log($"ConditionalDamageEffect: Condition met. Dealt {bonusDamage} bonus damage to {context.Target.Name}.");
                }
            }
        }
    }
}
