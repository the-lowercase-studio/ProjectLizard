using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IDragable : IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        event EventHandler<PointerEventData> OnDragStart;

        event EventHandler<PointerEventData> OnDragEnd;
    }
}
