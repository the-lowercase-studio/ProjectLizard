using Assets.CustomEventArgs;
using Assets.Energy;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace Assets.Cards
{
    public class CardsHandManager : MonoBehaviour
    {
        public static CardsHandManager Instance { get; private set; }

        public event EventHandler<EnumerableCollectionChangeEventArgs<ICard>> OnHandChange;

        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Transform _cardsHolder;

        private Queue<ICard> _cards = new();
        private EnergyManager _energyManager;

        private CardsHandManager()
        { }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _energyManager = EnergyManager.Instance;

            SetStartCards();
        }

        public ImmutableArray<ICard> GetCards()
        {
            return _cards.ToImmutableArray();
        }

        public void AddCard(CardConfigBaseSO config)
        {
            ICard card = Instantiate(_cardPrefab, _cardsHolder.transform);
            card.OnCardUsage += Card_OnCardUsage;
            card.Initialize(config);
            _cards.Enqueue(card);

            //TODO: CHANGE TYPE TO DIVIDE EXISTING COLLECTION AND ITEMS CHANGED
            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }

        private void Card_OnCardUsage(object sender, System.EventArgs e)
        {
            if (sender is ICard card)
            {
                _energyManager.CurrentEnergy -= card.GetCurrentEnergyCost();
            }
        }

        private void SetStartCards()
        {
            //TODO: change to random cards on start or some
            // other mechanizm like discarging cards form 10 on start and then setting them
            foreach (ICard card in _cardsHolder.GetComponentsInChildren<ICard>())
            {
                _cards.Enqueue(card);
            }

            Invoke("TestInvoke", 1f);
        }

        private void TestInvoke()
        {
            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }
    }
}
