using Assets.CustomEventArgs;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Inputs
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private GlobalInputActions globalInputActions;

        private InputAction pointerPositionAction;
        private InputAction pointerClick;
        public Vector2 PointerPositionInput { get; private set; }
        public static InputHandler Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                globalInputActions = new GlobalInputActions();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(Instance);
            }
        }

        private void OnEnable()
        {
            pointerPositionAction = globalInputActions.Player.PointerPosition;
            pointerPositionAction.performed += context => PointerPositionInput = context.ReadValue<Vector2>();
            pointerPositionAction.canceled += context => PointerPositionInput = Vector2.zero;
            pointerPositionAction.Enable();

            pointerClick = globalInputActions.Player.PointerClick;
            pointerClick.Enable();
        }

        private void OnDisable()
        {
            pointerPositionAction.Disable();
            pointerClick.Disable();
        }

        public void AddActionToOnPointerClick(Action<PointerClickEventArgs> action)
        {
            if (action == null)
            {
                return;
            }

            var pointerClickEventArgs = new PointerClickEventArgs(PointerPositionInput, Pointer.Instance.GetHoveredGameObject());
            pointerClick.performed += _ => action.Invoke(pointerClickEventArgs);
        }
    }
}
