using Assets.CustomEventArgs;
using Reflex.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Cards.Base;
using Assets.Cards.Constants;
using Assets.Turns;

namespace Assets.Cards.CardsHand
{
    public interface ICardsHandManager
    {
        event EventHandler<EnumerableCollectionChangeEventArgs<ICard>> OnHandChange;

        IEnumerable<ICard> GetCards();

        int CountCards();

        void AddCard(CardConfigBaseSO config);

        void RemoveCard(ICard card);
    }

    public class CardsHandManager : MonoBehaviour, ICardsHandManager
    {
        public event EventHandler<EnumerableCollectionChangeEventArgs<ICard>> OnHandChange;

        [Inject] private ITurnManager _turnManager;

        [SerializeField] private CardConfigBaseSO[] _testConfigs;
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Transform _cardsHolder;

        private List<ICard> _cards = new();

        public IEnumerator Start()
        {
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

        public IEnumerable<ICard> GetCards()
        {
            return _cards;
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
            for (int i = 0; i < CardConstants.Hand.START_CARDS_NUMBER; i++)
            {
                AddCard(_testConfigs[UnityEngine.Random.Range(0, _testConfigs.Length)]);
            }

            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }
    }
}
