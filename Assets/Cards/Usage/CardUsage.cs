using Assets.Cards.Base;
using Assets.Effects;
using Assets.Energy;
using System;
using UnityEngine;

namespace Assets.Cards.Usage
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

        private void OnEnable()
        {
            OnCardUsage += Card_OnCardUsage;
        }

        private void OnDisable()
        {
            OnCardUsage -= Card_OnCardUsage;
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

                OnCardUsage?.Invoke(_card, EventArgs.Empty);
            }
            else
            {
                Debug.Log($"No enrgy for card {_card.name} usage");
            }
        }

        private void Card_OnCardUsage(object sender, EventArgs e)
        {
            ExecuteEffects();
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

        private GameObject FindTarget()
        {
            // TODO: Implement target finding logic (raycast, closest enemy, etc.)
            return null;
        }
    }
}
