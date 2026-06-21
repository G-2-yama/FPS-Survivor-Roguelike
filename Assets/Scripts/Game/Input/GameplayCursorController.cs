using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayCursorController
{
    public void UpdateCursor(GameState currentState)
    {
        if (currentState == null)
        {
            return;
        }

        bool isCursorVisible = currentState.CursorActivationMode == CursorActivationMode.AlwaysVisible
            || IsModifierHeld();

        Cursor.visible = isCursorVisible;
        Cursor.lockState = isCursorVisible
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    private static bool IsModifierHeld()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
    }
}
