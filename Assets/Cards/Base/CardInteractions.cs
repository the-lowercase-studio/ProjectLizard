using Assets.Extensions;
using Assets.Inputs.Pointer;
using Assets.Interfaces.Interactions;
using Assets.UI;
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

        private EventTrigger _eventTrigger;
        private Card _card;
        private CardState _currentState = CardState.None;

        private void Awake()
        {
            _eventTrigger = GetComponentInChildren<EventTrigger>();
            _card = GetComponent<Card>();
        }

        private void OnEnable()
        {
            OnClick += Interactions_OnClick;
            OnHoverStart += Interactions_OnHoverStart;
            OnHoverEnd += Interactions_OnHoverEnd;
            OnDragStart += Interactions_OnDragStart;
            OnDragEnd += Interactions_OnDragEnd;

            _eventTrigger.triggers.Clear();
            _eventTrigger.triggers.AddEventHandlerInvocation(OnClick, EventTriggerType.PointerClick, _card);
            _eventTrigger.triggers.AddEventHandlerInvocation(OnHoverStart, EventTriggerType.PointerEnter, _card);
            _eventTrigger.triggers.AddEventHandlerInvocation(OnHoverEnd, EventTriggerType.PointerExit, _card);
            _eventTrigger.triggers.AddEventHandlerInvocation(OnDragStart, EventTriggerType.BeginDrag, _card);
            _eventTrigger.triggers.AddEventHandlerInvocation(OnDragEnd, EventTriggerType.EndDrag, _card);
        }

        private void OnDisable()
        {
            OnClick -= Interactions_OnClick;
            OnHoverStart -= Interactions_OnHoverStart;
            OnHoverEnd -= Interactions_OnHoverEnd;
            OnDragStart -= Interactions_OnDragStart;
            OnDragEnd -= Interactions_OnDragEnd;
        }

        private void Interactions_OnHoverStart(object sender, PointerEventData e)
        {
            Debug.Log("Hover start");

            if (CanTransitToHoveredState())
            {
                _currentState = CardState.Hovered;

                _card.Movement.MoveCardUp();
            }
        }

        private void Interactions_OnHoverEnd(object sender, PointerEventData e)
        {
            Debug.Log("Hover end");

            if (CanTransitToNoneState())
            {
                _currentState = CardState.None;

                CardsInHandPositioner.Instance.UpdateCardPlacement(_card);
            }
        }

        private void Interactions_OnDragStart(object sender, PointerEventData e)
        {
            Debug.Log("Drag start");

            if (CanTransitToDragState())
            {
                _card.Visual.SetParent(UITransformsProvider.Instance.FrontPanel);
                _card.Movement.VisualStartFollowingPointer();

                _currentState = CardState.Dragged;
            }
        }

        private void Interactions_OnDragEnd(object sender, PointerEventData e)
        {
            Debug.Log("Drag end");

            if (CanTransitToReturningToHandState())
            {
                _currentState = CardState.ReturningToHand;

                _card.Movement.VisualStopFollowingPointer();
                _card.Visual.SetParent(_card.transform);

                CardsInHandPositioner.Instance.UpdateCardPlacement(_card, () =>
                {
                    _currentState = CardState.None;

                    if (PointerHoverHelper.GetUIObjectsUnderPointer().FirstOrDefault().layer == 6)
                    {
                        OnHoverStart?.Invoke(this, e);
                    }
                });
            }
        }

        private void Interactions_OnClick(object sender, PointerEventData e)
        {
            Debug.Log($"{sender} on click");

            if (CanTransitToClickState())
            {
                _currentState = CardState.Clicked;

                //click logic

                _currentState = CardState.None;
            }
        }

        private bool CanTransitToHoveredState() => _currentState == CardState.None;

        private bool CanTransitToDragState() => _currentState == CardState.Hovered
                                                || _currentState == CardState.ReturningToHand;

        private bool CanTransitToClickState() => _currentState == CardState.Hovered;

        private bool CanTransitToReturningToHandState() => _currentState == CardState.Dragged;

        private bool CanTransitToNoneState() => _currentState == CardState.Hovered;
    }
}