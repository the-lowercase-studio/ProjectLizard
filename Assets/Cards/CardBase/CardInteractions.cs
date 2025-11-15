using Assets.Extensions;
using Assets.Interfaces.Interactions;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Cards.CardBase
{
    public interface ICardInteractions : IClickable, IHoverable, IDragable
    {
        void UpdateEventHandlers();
    }

    public class CardInteractions : MonoBehaviour, ICardInteractions
    {
        public event EventHandler<PointerEventData> OnClick;

        public event EventHandler<PointerEventData> OnHoverStart;

        public event EventHandler<PointerEventData> OnHoverEnd;

        public event EventHandler<PointerEventData> OnDragStart;

        public event EventHandler<PointerEventData> OnDragEnd;

        private EventTrigger _eventTrigger;

        private void Awake()
        {
            _eventTrigger = GetComponentInChildren<EventTrigger>();
        }

        public void UpdateEventHandlers()
        {
            _eventTrigger.triggers.Clear();
            _eventTrigger.AddEventHandler(OnClick, EventTriggerType.PointerClick, this);
            _eventTrigger.AddEventHandler(OnHoverStart, EventTriggerType.PointerEnter, this);
            _eventTrigger.AddEventHandler(OnHoverEnd, EventTriggerType.PointerExit, this);
            _eventTrigger.AddEventHandler(OnDragStart, EventTriggerType.BeginDrag, this);
            _eventTrigger.AddEventHandler(OnDragEnd, EventTriggerType.EndDrag, this);
        }
    }
}
