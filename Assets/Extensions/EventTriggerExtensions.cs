using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Assets.Extensions
{
    public static class EventTriggerExtensions
    {
        public static void AddEventHandlerInvocation(this List<EventTrigger.Entry> triggers, EventHandler<PointerEventData> eventHandler,
            EventTriggerType eventTriggerType, object caller = null)
        {
            EventTrigger.Entry entry = new();
            entry.eventID = eventTriggerType;
            entry.callback.AddListener((data) => eventHandler?.Invoke(caller, (PointerEventData)data));
            triggers.Add(entry);
        }
    }
}
