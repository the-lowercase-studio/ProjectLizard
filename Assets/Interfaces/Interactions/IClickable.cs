using System;
using UnityEngine.EventSystems;

namespace Assets.Interfaces.Interactions
{
    public interface IClickable : IPointerClickHandler
    {
        event EventHandler<PointerEventData> OnClick;
    }
}
