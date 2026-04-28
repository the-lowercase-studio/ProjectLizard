using Assets.Cards.Base.Targeting;
using Assets.Effects.StatusEffects;
using Assets.ElementalSystem;
using Assets.Targeting;
using UnityEngine;

namespace Assets.Cards.Base.Damage
{
    public interface ICardDamage
    {
        bool TryApplyDamage(CardResolvedHit resolvedHit);
    }

    [RequireComponent(typeof(Card))]
    public class CardDamage : MonoBehaviour, ICardDamage
    {
        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public bool TryApplyDamage(CardResolvedHit resolvedHit)
        {
            if (resolvedHit.Step?.Damage == null || !IsTargetAlive(resolvedHit.Target))
            {
                return false;
            }

            int modifiedDamage = GetModifiedDamageByStatusEffects(resolvedHit.Target, resolvedHit.Step.Damage.DamageValue, _card.Config.Element);
            resolvedHit.Target.Damageable.TakeDamage(modifiedDamage);
            return true;
        }

        public static int GetModifiedDamageByStatusEffects(ITarget target, int baseDamage, Elements damageElement)
        {
            if (target?.StatusEffectReceiver == null)
            {
                return baseDamage;
            }

            int modifiedDamage = baseDamage;

            foreach (IStatusEffectBase effect in target.StatusEffectReceiver.GetActiveEffects())
            {
                if (effect is IIncomingDamageModifier incomingDamageModifier)
                {
                    modifiedDamage = incomingDamageModifier.ModifyIncomingDamage(modifiedDamage, damageElement);
                }
            }

            return modifiedDamage;
        }

        public static bool IsTargetAlive(ITarget target)
        {
            return target?.Damageable?.Health != null && target.Damageable.Health.IsAlive();
        }
    }
}
