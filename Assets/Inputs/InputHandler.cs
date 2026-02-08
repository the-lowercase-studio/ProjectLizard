using Assets.CustomEventArgs;
using Assets.Inputs.Pointer;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Inputs
{
    public interface IInputHandler
    {
        Vector2 PointerPositionInput { get; }

        void AddActionToOnPointerClick(Action<PointerClickEventArgs> action);
    }

    public class InputHandler : MonoBehaviour, IInputHandler
    {
        public Vector2 PointerPositionInput { get; private set; }

        [SerializeField] private GlobalInputActions globalInputActions;

        private InputAction pointerPositionAction;
        private InputAction pointerClick;

        private void Awake()
        {
            globalInputActions = new GlobalInputActions();
            DontDestroyOnLoad(gameObject);
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

            var pointerClickEventArgs = new PointerClickEventArgs(PointerPositionInput, PointerHoverHelper.GetHoveredGameObject(PointerPositionInput));
            pointerClick.performed += _ => action.Invoke(pointerClickEventArgs);
        }
    }
}