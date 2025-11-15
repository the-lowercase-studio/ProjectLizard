using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IDroppable
    {
        event EventHandler<PointerEventData> OnDrop;
    }
}