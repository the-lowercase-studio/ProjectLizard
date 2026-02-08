using Assets.Cards.CardsHand;
using Assets.Inputs.Pointer;
using Assets.Interfaces.Interactions;
using Assets.UI;
using Reflex.Attributes;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Cards.Base
{
    public interface ICardInteractions : IClickable, IHoverable, IDragable
    {
    }

    [RequireComponent(typeof(Card))]
    public class CardInteractions : MonoBehaviour, ICardInteractions
    {
        public event EventHandler<PointerEventData> OnClick;

        public event EventHandler<PointerEventData> OnHoverStart;

        public event EventHandler<PointerEventData> OnHoverEnd;

        public event EventHandler<PointerEventData> OnDragStart;

        public event EventHandler<PointerEventData> OnDragEnd;

        [Inject] private ICardsHandPresenter _cardsHandPresenter;
        [Inject] private IUITransformsProvider _uiTransformsProvider;
        [Inject] private IPointerPositioner _pointerPositioner;

        private Card _card;
        private CardState _currentState = CardState.None;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _card = GetComponent<Card>();
            _canvasGroup = _card.Visual.GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            OnDragStart += Interactions_OnDragStart;
            OnDragEnd += Interactions_OnDragEnd;
        }

        private void OnDisable()
        {
            OnDragStart -= Interactions_OnDragStart;
            OnDragEnd -= Interactions_OnDragEnd;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this, eventData);
            Debug.Log($"{_card.name} on click");

            if (CanTransitToClickState())
            {
                _currentState = CardState.Clicked;

                //click logic

                _currentState = CardState.Hovered;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverStart?.Invoke(this, eventData);
            Debug.Log("Hover start");

            if (CanTransitToHoveredState())
            {
                _currentState = CardState.Hovered;

                _card.Movement.LiftCardUp();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverEnd?.Invoke(this, eventData);
            Debug.Log("Hover end");

            if (CanTransitToNoneState())
            {
                _currentState = CardState.None;

                _cardsHandPresenter.UpdateCardPlacement(_card);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        private void Interactions_OnDragStart(object sender, PointerEventData e)
        {
            _canvasGroup.blocksRaycasts = false;
        }

        private void Interactions_OnDragEnd(object sender, PointerEventData e)
        {
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDragStart?.Invoke(this, eventData);
            Debug.Log("Drag start");

            if (CanTransitToDragState())
            {
                _card.Visual.transform.SetParent(_uiTransformsProvider.FrontPanel);
                _card.Movement.VisualStartFollowingPointer();

                _currentState = CardState.Dragged;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnd?.Invoke(this, eventData);
            Debug.Log("Drag end");

            if (CanTransitToReturningToHandState())
            {
                _currentState = CardState.ReturningToHand;

                _card.Movement.VisualStopFollowingPointer();
                _card.Visual.transform.SetParent(_card.transform);

                _cardsHandPresenter.UpdateCardPlacement(_card, () =>
                {
                    var hoveredObjects = PointerHoverHelper
                        .GetUIObjectsUnderPointer(_pointerPositioner.RawInputPosition)
                        .Where(o => o.layer == 6);

                    if (hoveredObjects.Any(o => o.transform.parent == _card.Visual))
                    {
                        _currentState = CardState.Hovered;
                        _card.Movement.LiftCardUp();
                        OnHoverStart?.Invoke(this, eventData);
                    }
                    else
                    {
                        _currentState = CardState.None;
                    }
                });
            }
        }

        private bool CanTransitToHoveredState()
        {
            return _currentState == CardState.None;
        }

        private bool CanTransitToDragState()
        {
            return _currentState == CardState.Hovered
                || _currentState == CardState.ReturningToHand;
        }

        private bool CanTransitToClickState()
        {
            return _currentState == CardState.Hovered;
        }

        private bool CanTransitToReturningToHandState()
        {
            return _currentState == CardState.Dragged;
        }

        private bool CanTransitToNoneState()
        {
            return _currentState == CardState.Hovered;
        }
    }
}
