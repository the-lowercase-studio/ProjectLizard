using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IDropHandler
    {
        event EventHandler<PointerEventData> OnDrop;
    }
}