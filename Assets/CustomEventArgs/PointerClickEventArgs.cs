using System;
using UnityEngine;

namespace Assets.CustomEventArgs
{
    public class PointerClickEventArgs : EventArgs
    {
        public Vector2 PointerPosition { get; private set; }
        public GameObject ClickedObject { get; private set; }

        public PointerClickEventArgs(Vector2 pointerPosition, GameObject clickedObject)
        {
            PointerPosition = pointerPosition;
            ClickedObject = clickedObject;
        }
    }
}
