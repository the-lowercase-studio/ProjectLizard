using Assets.CustomEventArgs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using Assets.Cards.Base;
using Assets.Turns;
using Assets.Cards.Constants;

namespace Assets.Cards.CardsHand
{
    public class CardsHandManager : MonoBehaviour
    {
        public static CardsHandManager Instance { get; private set; }

        public event EventHandler<EnumerableCollectionChangeEventArgs<ICard>> OnHandChange;

        [SerializeField] private CardConfigBaseSO _testConfig;
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Transform _cardsHolder;

        private List<ICard> _cards = new();

        private TurnManager _turnManager;

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

        //private void OnEnable()
        //{
        //    _energyManager = EnergyManager.Instance;

        //    Canvas.willRenderCanvases += Canvas_willRenderCanvases;
        //}

        //private void OnDisable()
        //{
        //    Canvas.willRenderCanvases -= Canvas_willRenderCanvases;
        //}

        public IEnumerator Start()
        {
            _turnManager = TurnManager.Instance;
            _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
            _turnManager.OnPlayerTurnEnd += TurnManager_OnPlayerTurnEnd;

            yield return new WaitForEndOfFrame();

            Canvas_willRenderCanvases();
        }

        private void OnDisable()
        {
            _turnManager.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
            _turnManager.OnPlayerTurnEnd -= TurnManager_OnPlayerTurnEnd;
        }

        public ImmutableArray<ICard> GetCards()
        {
            return _cards.ToImmutableArray();
        }

        public int CountCards()
        {
            return _cards.Count;
        }

        public void AddCard(CardConfigBaseSO config)
        {
            ICard card = Instantiate(_cardPrefab, _cardsHolder.transform);
            card.OnCardDiscard += Card_OnCardDiscard;
            card.Initialize(config);
            _cards.Add(card);

            //TODO: CHANGE TYPE TO DIVIDE EXISTING COLLECTION AND ITEMS CHANGED
            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }

        public void RemoveCard(ICard card)
        {
            card.OnCardDiscard -= Card_OnCardDiscard;
            _cards.Remove(card);

            //TODO: CHANGE TYPE TO DIVIDE EXISTING COLLECTION AND ITEMS CHANGED
            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }

        private void Card_OnCardDiscard(object sender, EventArgs e)
        {
            if (sender is ICard card)
            {
                Debug.Log(sender + " Card discarded");

                RemoveCard(card);
            }
        }

        private void Canvas_willRenderCanvases()
        {
            FillHand();
        }

        private void TurnManager_OnPlayerTurnStart(object sender, EventArgs e)
        {
            FillHand();
        }

        private void TurnManager_OnPlayerTurnEnd(object sender, EventArgs e)
        {
            DiscardHand();
        }

        private void DiscardHand()
        {
            List<ICard> cardsCopy = new(_cards);
            foreach (var card in cardsCopy)
            {
                card.Discard();
            }
        }

        private void FillHand()
        {
            for (int i = 0; i < CardConstants.START_CARDS_NUMBER; i++)
            {
                AddCard(_testConfig);
            }

            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }
    }
}
