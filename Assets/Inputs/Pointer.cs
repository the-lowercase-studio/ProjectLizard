using UnityEngine;

namespace Assets.Inputs
{
    public class Pointer : MonoBehaviour
    {
        public static Pointer Instance { get; private set; }
        public static Vector2 ScreenPosition { get; private set; }
        public static Vector2 WorldPosition { get; private set; }

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

        public GameObject GetHoveredGameObject()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_inputHandler.PointerPositionInput);

            if (Physics.Raycast(ray, out var hit))
            {
                return hit.transform.gameObject;
            }

            return null;
        }

        private void UpdatePositions()
        {
            ScreenPosition = _mainCamera.ScreenToWorldPoint(_inputHandler.PointerPositionInput);
            WorldPosition = _inputHandler.PointerPositionInput;
        }
    }
}
