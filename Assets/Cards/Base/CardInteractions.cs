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
        private ICardInteractionStateMachine _interactionStateMachine;
        private CanvasGroup _canvasGroup;
        private Canvas _visualCanvas;

        private void Awake()
        {
            _card = GetComponent<Card>();
            _canvasGroup = _card.Visual.GetComponent<CanvasGroup>();
            _visualCanvas = _card.Visual.GetComponent<Canvas>();
            _interactionStateMachine = new CardInteractionStateMachine(() => _cardDragLock.IsAnyCardBeingDragged);
        }

        private void OnEnable()
        {
            _visualCanvas.overrideSorting = false;
        }

        private void OnDisable()
        {
            if (_interactionStateMachine.CurrentState == CardState.Dragged)
            {
                _cardDragLock.Release();
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this, eventData);
            Debug.Log($"{_card.name} on click");

            if (_interactionStateMachine.TryTransitionToClicked(() =>
            {
                //click logic
            }))
            {
                _interactionStateMachine.ForceTransition(CardState.Hovered);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverStart?.Invoke(this, eventData);
            Debug.Log("Hover start");

            _interactionStateMachine.TryTransitionToHovered(() =>
            {
                _visualCanvas.overrideSorting = true;
                _visualCanvas.sortingOrder = LayersOrder.Cards.INTERACTED_LAYER_ORDER;

                _card.Scaler.SetVisualPivotToBottom();
                _card.Movement.AlignVisualWithRoot(withTweening: true);
                _card.Rotation.SetZVisualRotation(0f, withTweening: true);
                _card.Scaler.ScaleVisualUp();
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverEnd?.Invoke(this, eventData);
            Debug.Log("Hover end");

            _interactionStateMachine.TryTransitionToNone(() =>
            {
                _visualCanvas.overrideSorting = false;

                _card.Scaler.ResetVisualScaleFromBottom();
                _cardsHandPresenter.UpdateCardPlacement(_card);
            });
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_interactionStateMachine.CanTransitionToDragged() && _cardDragLock.TryAcquire())
            {
                _interactionStateMachine.TryTransitionToDragged(() =>
                {
                    Debug.Log("Drag start");
                    _visualCanvas.overrideSorting = true;
                    _visualCanvas.sortingOrder = LayersOrder.Cards.INTERACTED_LAYER_ORDER;
                    _card.Movement.VisualStartFollowingPointer();
                    _card.Scaler.ResetVisualScaleFromBottom();
                    _canvasGroup.blocksRaycasts = false;

                    OnDragStart?.Invoke(this, eventData);
                });
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _interactionStateMachine.TryTransitionToReturningToHand(() =>
            {
                Debug.Log("Drag end");

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
                        _interactionStateMachine.ForceTransition(CardState.Hovered, () =>
                        {
                            _visualCanvas.overrideSorting = true;
                            _visualCanvas.sortingOrder = LayersOrder.Cards.INTERACTED_LAYER_ORDER;

                            _card.Scaler.SetVisualPivotToBottom();
                            _card.Movement.AlignVisualWithRoot();
                            _card.Rotation.SetZVisualRotation(0f, withTweening: true);
                            _card.Scaler.ScaleVisualUp();

                            OnHoverStart?.Invoke(this, eventData);
                        });
                    }
                    else
                    {
                        _interactionStateMachine.ForceTransition(CardState.None, () =>
                        {
                            _visualCanvas.overrideSorting = false;

                            _card.Scaler.ResetVisualScale();
                        });
                    }
                });
            });
        }
    }
}
