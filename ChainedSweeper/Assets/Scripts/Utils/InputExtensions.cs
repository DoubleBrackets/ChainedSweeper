using System;
using UnityEngine.InputSystem;

namespace Utils
{
    public static class InputExtensions
    {
        public static void SubscribeToAllAndEnable(this InputActionReference actionRef,
            Action<InputAction.CallbackContext> callback)
        {
            actionRef.action.started += callback;
            actionRef.action.performed += callback;
            actionRef.action.canceled += callback;
            actionRef.Enable();
        }

        public static void UnsubscribeToAllAndDisable(this InputActionReference actionRef,
            Action<InputAction.CallbackContext> callback)
        {
            actionRef.action.started -= callback;
            actionRef.action.performed -= callback;
            actionRef.action.canceled -= callback;
            actionRef.Disable();
        }

        public static void Enable(this InputActionReference reference)
        {
            reference.action.Enable();
        }

        public static void Disable(this InputActionReference reference)
        {
            reference.action.Disable();
        }
    }
}