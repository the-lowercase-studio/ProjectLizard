using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IDragable
    {
        event EventHandler<PointerEventData> OnDragStart;

        event EventHandler<PointerEventData> OnDragEnd;
    }
}
