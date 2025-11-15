using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IHoverable
    {
        event EventHandler<PointerEventData> OnHoverStart;

        event EventHandler<PointerEventData> OnHoverEnd;
    }
}
