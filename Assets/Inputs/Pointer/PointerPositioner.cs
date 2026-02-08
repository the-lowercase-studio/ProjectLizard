using Reflex.Attributes;
using UnityEngine;

namespace Assets.Inputs.Pointer
{
    public interface IPointerPositioner
    {
        Vector2 ScreenPosition { get; }
        Vector2 RawInputPosition { get; }
    }

    public class PointerPositioner : MonoBehaviour, IPointerPositioner
    {
        public Vector2 ScreenPosition { get; private set; }
        public Vector2 RawInputPosition { get; private set; }

        [Inject] private IInputHandler _inputHandler;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            ScreenPosition = _mainCamera.ScreenToWorldPoint(_inputHandler.PointerPositionInput);
            RawInputPosition = _inputHandler.PointerPositionInput;
        }
    }
}
