using Assets.Effects.Base;
using Assets.Energy;
using Assets.Targeting;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Cards.Base.Usage
{
    public interface ICardUsage
    {
        event EventHandler OnCardUsage;

        void Use();
    }

    [RequireComponent(typeof(Card))]
    public class CardUsage : MonoBehaviour, ICardUsage
    {
        public event EventHandler OnCardUsage;

        [Inject] private IEnergyManager _energyManager;
        [Inject] private ITargetsProvider _targetsManager;
        [Inject] private IPlayerParty _playerParty;

        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void Use()
        {
            int currentEnergyCost = _card.GetCurrentEnergyCost();
            if (currentEnergyCost <= _energyManager.CurrentEnergy)
            {
                Debug.Log($"Card {_card.name} used");

                _energyManager.DecreaseCurrentEnergy(currentEnergyCost);

                TryPlayAttackAnimation();

                ExecuteDamage();
                ExecuteEffects();

                _card.Discard();
            }
            else
            {
                Debug.Log($"No enrgy for card {_card.name} usage");
            }
        }

        private void ExecuteDamage()
        {
            Debug.Log("CARD DAMAGE: " + _card.CardDamage);
            _card.CardDamage?.Execute(_targetsManager);
        }

        private void TryPlayAttackAnimation()
        {
            if (_playerParty == null || _card?.Config == null)
            {
                return;
            }

            List<PartyCharacter> characters = _playerParty.GetAllCharacters();
            if (characters == null)
            {
                return;
            }

            foreach (PartyCharacter character in characters)
            {
                if (character != null && character.TryPlayAttackAnimationForElement(_card.Config.Element))
                {
                    break;
                }
            }
        }

        private void ExecuteEffects()
        {
            CardEffectContext context = CreateContext();

            foreach (EffectSO effect in _card.Config.Effects)
            {
                effect.Execute(context);
            }

            Debug.Log($"Card '{_card.Config.Title}' executed {_card.Config.Effects.Count} effect(s).");
        }

        private CardEffectContext CreateContext()
        {
            return new CardEffectContext
            {
                Source = gameObject,
                Position = transform.position,
                Target = FindTarget(),
                TargetsProvider = _targetsManager
            };
        }

        private ITarget FindTarget()
        {
            //TODO: change for different modes based on card ability
            return _targetsManager.GetFirst();
        }
    }
}
