using System.Collections.Generic;
using UnityEngine;

namespace Assets.Cards
{
    public class CardsHand : MonoBehaviour
    {
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Transform _cardsHolder;
        private Queue<Card> _cards;

        public void AddCard(CardBaseSO config)
        {
            Card card = Instantiate(_cardPrefab, _cardsHolder.transform);
            card.Initialize(config);
            _cards.Enqueue(card);
        }
    }
}
