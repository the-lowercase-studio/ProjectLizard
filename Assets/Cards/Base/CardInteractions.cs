using Assets.Cards.CardsHand;
using Assets.Cards.Base.Interaction;
using Assets.Cards.Constants;
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
        [Inject] private IPointerPositioner _pointerPositioner;
        [Inject] private ICardDragLock _cardDragLock;

        private Card _card;
        private CardState _currentState = CardState.None;
        private CanvasGroup _canvasGroup;
        private Canvas _visualCanvas;

        private void Awake()
        {
            _card = GetComponent<Card>();
            _canvasGroup = _card.Visual.GetComponent<CanvasGroup>();
            _visualCanvas = _card.Visual.GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            _visualCanvas.overrideSorting = true;
            _visualCanvas.sortingOrder = LayersOrder.Cards.DEFAULT_LAYER_ORDER;
        }

        private void OnDisable()
        {
            if (_currentState == CardState.Dragged)
            {
                _cardDragLock.Release();
                _canvasGroup.blocksRaycasts = true;
            }
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

                _visualCanvas.sortingOrder = LayersOrder.Cards.INTERACTED_LAYER_ORDER;

                _card.Scaler.SetVisualPivotToBottom();
                _card.Movement.AlignVisualWithRoot(withTweening: true);
                _card.Rotation.SetZVisualRotation(0f, withTweening: true);
                _card.Scaler.ScaleVisualUp();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverEnd?.Invoke(this, eventData);
            Debug.Log("Hover end");

            if (CanTransitToNoneState())
            {
                _currentState = CardState.None;

                _visualCanvas.sortingOrder = LayersOrder.Cards.DEFAULT_LAYER_ORDER;

                _card.Scaler.ResetVisualScaleFromBottom();
                _cardsHandPresenter.UpdateCardPlacement(_card);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CanTransitToDragState() && _cardDragLock.TryAcquire())
            {
                Debug.Log("Drag start");
                _card.Movement.VisualStartFollowingPointer();
                _card.Scaler.ResetVisualScaleFromBottom();
                _canvasGroup.blocksRaycasts = false;

                _currentState = CardState.Dragged;
                OnDragStart?.Invoke(this, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (CanTransitToReturningToHandState())
            {
                Debug.Log("Drag end");
                _currentState = CardState.ReturningToHand;

                _card.Movement.VisualStopFollowingPointer();
                _cardDragLock.Release();
                _canvasGroup.blocksRaycasts = true;
                OnDragEnd?.Invoke(this, eventData);

                _cardsHandPresenter.UpdateCardPlacement(_card, () =>
                {
                    var hoveredObjects = PointerHoverHelper
                        .GetUIObjectsUnderPointer(_pointerPositioner.RawInputPosition)
                        .Where(o => o.layer == 6);

                    if (hoveredObjects.Any(o => o.transform.parent == _card.Visual))
                    {
                        _currentState = CardState.Hovered;

                        _visualCanvas.sortingOrder = LayersOrder.Cards.INTERACTED_LAYER_ORDER;

                        _card.Scaler.SetVisualPivotToBottom();
                        _card.Movement.AlignVisualWithRoot();
                        _card.Rotation.SetZVisualRotation(0f, withTweening: true);
                        _card.Scaler.ScaleVisualUp();

                        OnHoverStart?.Invoke(this, eventData);
                    }
                    else
                    {
                        _currentState = CardState.None;

                        _visualCanvas.sortingOrder = LayersOrder.Cards.DEFAULT_LAYER_ORDER;

                        _card.Scaler.ResetVisualScale();
                    }
                });
            }
        }

        private bool CanTransitToHoveredState()
        {
            return _currentState == CardState.None && !_cardDragLock.IsAnyCardBeingDragged;
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
