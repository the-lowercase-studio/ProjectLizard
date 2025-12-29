using UnityEngine;

namespace Assets.Inputs.Pointer
{
    public class PointerPositioner : MonoBehaviour
    {
        public static PointerPositioner Instance { get; private set; }
        public static Vector2 ScreenPosition { get; private set; }
        public static Vector2 RawInputPosition { get; private set; }

        private InputHandler _inputHandler;
        private Camera _mainCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _mainCamera = Camera.main;
        }

        private void Start()
        {
            _inputHandler = InputHandler.Instance;
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