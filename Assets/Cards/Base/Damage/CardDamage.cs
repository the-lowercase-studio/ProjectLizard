using Assets.Effects.StatusEffects;
using Assets.ElementalSystem;
using Assets.Targeting;
using System.Linq;
using UnityEngine;

namespace Assets.Cards.Base.Damage
{
    public interface ICardDamage
    {
        void Execute(ITargetsProvider targetsProvider);
    }

    [RequireComponent(typeof(Card))]
    public class CardDamage : MonoBehaviour, ICardDamage
    {
        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void Execute(ITargetsProvider targetsProvider)
        {
            var config = _card.Config.Damage;

            if (config == null)
                return;

            switch (config.TargetMode)
            {
                case TargetingMode.Same:
                    ExecuteSameTarget(config, targetsProvider);
                    break;

                case TargetingMode.Other:
                    ExecuteOtherTargets(config, targetsProvider);
                    break;
            }
        }

        private void ExecuteSameTarget(CardDamageSO config, ITargetsProvider targetsProvider)
        {
            Debug.Log("EXECUTED ON SAME TARGET " + _card.name + " " + _card.Config.Damage.DamageValue);

            var target = targetsProvider.GetFromStartPosition(config.StartPosition, 1).FirstOrDefault();

            if (target == null)
                return;

            for (int i = 0; i < config.AttackCount; i++)
            {
                int modifiedDamage = GetModifiedDamageByStatusEffects(target, config.DamageValue, _card.Config.Element);
                target.Damageable.TakeDamage(modifiedDamage);
            }
        }

        private void ExecuteOtherTargets(CardDamageSO config, ITargetsProvider targetsProvider)
        {
            var targets = targetsProvider.GetFromStartPosition(config.StartPosition, config.AttackCount);

            foreach (var target in targets)
            {
                int modifiedDamage = GetModifiedDamageByStatusEffects(target, config.DamageValue, _card.Config.Element);
                target.Damageable.TakeDamage(modifiedDamage);
            }
        }

        private static int GetModifiedDamageByStatusEffects(ITarget target, int baseDamage, Elements damageElement)
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
