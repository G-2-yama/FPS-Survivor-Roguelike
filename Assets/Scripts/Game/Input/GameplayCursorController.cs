using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲーム状態と入力元に応じてOSカーソルを制御する
/// </summary>
public sealed class GameplayCursorController
{
    /// <summary>
    /// 現在のゲーム状態でカーソル操作が許可されているかを返す
    /// </summary>
    public bool IsCursorAllowed(GameState currentState)
    {
        if (currentState == null)
        {
            return false;
        }

        return currentState.CursorActivationMode == CursorActivationMode.AlwaysVisible
            || IsModifierHeld();
    }

    /// <summary>
    /// 物理マウスと仮想カーソルのどちらを表示するか反映する
    /// </summary>
    public void UpdateCursor(GameState currentState, bool gamepadOwnsPointer)
    {
        bool cursorAllowed = IsCursorAllowed(currentState);
        Cursor.visible = cursorAllowed && !gamepadOwnsPointer;
        Cursor.lockState = cursorAllowed
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    /// <summary>
    /// Altキーのいずれかが押されているかを調べる
    /// </summary>
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
