using UnityEngine;
using UnityEngine.InputSystem;

namespace PivotAscent
{
    public static class PivotInput
    {
        public static bool TapDown =>
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        public static Vector2 PointerPosition => Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed
            ? Touchscreen.current.primaryTouch.position.ReadValue()
            : Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }
}
