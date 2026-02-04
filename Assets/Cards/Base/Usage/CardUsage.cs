using Assets.Effects;
using Assets.Energy;
using Assets.Targeting;
using System;
using System.Linq;
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

        private Card _card;
        private EnergyManager _energyManager;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        private void Start()
        {
            _energyManager = EnergyManager.Instance;
        }

        public void Use()
        {
            int currentEnergyCost = _card.GetCurrentEnergyCost();
            if (currentEnergyCost <= _energyManager.CurrentEnergy)
            {
                Debug.Log($"Card {_card.name} used");

                _energyManager.DecreaseCurrentEnergy(currentEnergyCost);

                ExecuteEffects();

                _card.Discard();
            }
            else
            {
                Debug.Log($"No enrgy for card {_card.name} usage");
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
                Target = FindTarget()
            };
        }

        private ITarget FindTarget()
        {
            //TODO: change for different modes based on card ability
            return TargetsManager.Instance.GetTargets(TargetsMode.First).First();
        }
    }
}
