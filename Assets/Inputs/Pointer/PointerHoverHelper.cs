using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Inputs.Pointer
{
    public static class PointerHoverHelper
    {
        public static IEnumerable<GameObject> GetUIObjectsUnderPointer(Vector2 pointerPosition)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);

            eventData.position = pointerPosition;

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(eventData, results);

            return results.Select(item => item.gameObject);
        }

        public static GameObject GetHoveredGameObject(Vector2 pointerPositionInput)
        {
            Ray ray = Camera.main.ScreenPointToRay(pointerPositionInput);

            if (Physics.Raycast(ray, out var hit))
            {
                return hit.transform.gameObject;
            }

            return null;
        }
    }
}
