using System;
using UnityEngine.EventSystems;

namespace Assets.Extensions
{
    public static class EventTriggerExtensions
    {
        public static void AddEventHandler(this EventTrigger eventTrigger, EventHandler<PointerEventData> eventHandler,
            EventTriggerType eventTriggerType, object caller = null)
        {
            EventTrigger.Entry entry = new();
            entry.eventID = eventTriggerType;
            entry.callback.AddListener((data) => eventHandler?.Invoke(caller, (PointerEventData)data));
            eventTrigger.triggers.Add(entry);
        }
    }
}
