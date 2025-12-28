using Assets.Extensions;
using Assets.Interfaces.Interactions;
using Assets.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Cards.CardBase.Interactions
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
        private CardInteraction _currentInteraction = CardInteraction.None;

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

            _eventTrigger.triggers.Clear();
        }

        private void Interactions_OnHoverStart(object sender, PointerEventData e)
        {
            if (CanTransitToHoverStartInteraction())
            {
                Debug.Log("Hover start");

                _card.Movement.MoveCardUp();

                _currentInteraction = CardInteraction.Hover;
            }
        }

        private void Interactions_OnHoverEnd(object sender, PointerEventData e)
        {
            if (CanTransitToHoverEndInteraction())
            {
                Debug.Log("Hover end");

                CardsInHandPositioner.Instance.UpdateCardPlacement(
                    _card,
                    () =>
                    {
                        _currentInteraction = CardInteraction.None;
                    }
                );

                _currentInteraction = CardInteraction.None;
            }
        }

        private void Interactions_OnDragStart(object sender, PointerEventData e)
        {
            if (CanTransitToDragStartInteraction())
            {
                Debug.Log("Drag start");

                _card.Visual.SetParent(UITransformsProvider.Instance.FrontPanel);
                _card.Movement.VisualStartFollowingPointer();

                _currentInteraction = CardInteraction.Drag;
            }
        }

        private void Interactions_OnDragEnd(object sender, PointerEventData e)
        {
            if (CanTransitToDragEndInteraction())
            {
                Debug.Log("Drag end");

                _card.Movement.VisualStopFollowingPointer();
                _card.Visual.SetParent(_card.transform);
                CardsInHandPositioner.Instance.UpdateCardPlacement(_card);

                _currentInteraction = CardInteraction.None;
            }
        }

        private void Interactions_OnClick(object sender, PointerEventData e)
        {
            if (CanTransitToClickInteraction())
            {
                _currentInteraction = CardInteraction.Click;

                //click logic
                Debug.Log($"{sender} on click");

                _currentInteraction = CardInteraction.None;
            }
        }

        private bool CanTransitToHoverStartInteraction() => _currentInteraction == CardInteraction.None;

        private bool CanTransitToHoverEndInteraction() => _currentInteraction == CardInteraction.Hover
                                                            || _currentInteraction == CardInteraction.Click;

        private bool CanTransitToDragStartInteraction() => _currentInteraction == CardInteraction.Hover;

        private bool CanTransitToDragEndInteraction() => _currentInteraction == CardInteraction.Drag;

        private bool CanTransitToClickInteraction() => _currentInteraction == CardInteraction.Hover;
    }
}
