using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IClickable
    {
        event EventHandler<PointerEventData> OnClick;
    }
}
