using Assets.Extensions;
using Assets.Inputs;
using Assets.Interfaces.Interactions;
using Assets.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Cards.CardBase
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
        private bool _isCardDragged;

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
            if (_isCardDragged)
            {
                return;
            }

            _card.Movement.MoveCardUp();
        }

        private void Interactions_OnHoverEnd(object sender, PointerEventData e)
        {
            if (_isCardDragged)
            {
                return;
            }

            CardsInHandPositioner.Instance.UpdateCardPlacement(_card);
        }

        private void Interactions_OnDragStart(object sender, PointerEventData e)
        {
            _card.Visual.SetParent(UITransformsProvider.Instance.BackPanel);
            _card.Movement.VisualStartFollowingPointer();
            _isCardDragged = true;
        }

        private void Interactions_OnDragEnd(object sender, PointerEventData e)
        {
            _card.Movement.VisualStopFollowingPointer();
            _card.Visual.SetParent(_card.transform);
            CardsInHandPositioner.Instance.UpdateCardPlacement(_card);
            _isCardDragged = false;
        }

        private void Interactions_OnClick(object sender, PointerEventData e)
        {
            Debug.Log($"{sender} on click");
        }
    }
}
