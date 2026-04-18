using Assets.Effects.StatusEffects;
using Assets.ElementalSystem;
using Assets.Cards.Base.Targeting;
using Assets.Targeting;
using Reflex.Attributes;
using UnityEngine;

namespace Assets.Cards.Base.Damage
{
    public interface ICardDamage
    {
        void Execute();
    }

    [RequireComponent(typeof(Card))]
    public class CardDamage : MonoBehaviour, ICardDamage
    {
        [Inject] private ITargetsProvider _targetsProvider;
        [Inject] private ICardTargetResolver _cardTargetResolver;

        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void Execute()
        {
            var config = _card.Config.Damage;

            if (config == null)
            {
                return;
            }

            var targetSelections = _cardTargetResolver.ResolveDamageTargets(_targetsProvider, _card.Config);

            foreach (CardDamageTargetSelection targetSelection in targetSelections)
            {
                if (targetSelection.Target == null)
                {
                    continue;
                }

                for (int hitIndex = 0; hitIndex < targetSelection.HitCount; hitIndex++)
                {
                    int modifiedDamage = GetModifiedDamageByStatusEffects(targetSelection.Target, config.DamageValue, _card.Config.Element);
                    targetSelection.Target.Damageable.TakeDamage(modifiedDamage);
                }
            }
        }

        public static int GetModifiedDamageByStatusEffects(ITarget target, int baseDamage, Elements damageElement)
        {
            if (target?.StatusEffectReceiver == null)
            {
                return baseDamage;
            }

            int modifiedDamage = baseDamage;

            foreach (IStatusEffect effect in target.StatusEffectReceiver.GetActiveEffects())
            {
                if (effect is IIncomingDamageModifier incomingDamageModifier)
                {
                    modifiedDamage = incomingDamageModifier.ModifyIncomingDamage(modifiedDamage, damageElement);
                }
            }

            return modifiedDamage;
        }
    }
}
