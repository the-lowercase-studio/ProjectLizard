using Assets.CustomEventArgs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using Assets.Cards.Base;

namespace Assets.Cards
{
    public class CardsHandManager : MonoBehaviour
    {
        public static CardsHandManager Instance { get; private set; }

        public event EventHandler<EnumerableCollectionChangeEventArgs<ICard>> OnHandChange;

        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Transform _cardsHolder;

        private List<ICard> _cards = new();

        private CardsInHandPositioner _cardsInHandPositioner;

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
            _cardsInHandPositioner = CardsInHandPositioner.Instance;

            yield return new WaitForEndOfFrame();

            Canvas_willRenderCanvases();
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
            card.OnCardDiscard += Card_OnCardDiscard; ;
            card.Initialize(config);
            _cards.Add(card);

            //TODO: CHANGE TYPE TO DIVIDE EXISTING COLLECTION AND ITEMS CHANGED
            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }

        private void Card_OnCardDiscard(object sender, EventArgs e)
        {
            if (sender is ICard card)
            {
                Debug.Log(sender + " Card discarded");

                RemoveCard(card);

                _cardsInHandPositioner.UpdateAllCardsPlacement();

                OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
            }
        }

        public void RemoveCard(ICard card)
        {
            _cards.Remove(card);

            //TODO: CHANGE TYPE TO DIVIDE EXISTING COLLECTION AND ITEMS CHANGED
            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }

        private void Canvas_willRenderCanvases()
        {
            SetStartCards();
        }

        private void SetStartCards()
        {
            _cards.AddRange(_cardsHolder.GetComponentsInChildren<ICard>());

            OnHandChange?.Invoke(this, new EnumerableCollectionChangeEventArgs<ICard>(_cards));
        }
    }
}
