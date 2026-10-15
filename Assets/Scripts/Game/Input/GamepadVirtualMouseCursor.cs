using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

/// <summary>
/// 物理マウスとゲームパッド仮想カーソルの入力元を切り替える
/// ホバー処理は InputSystemUIInputModule に任せる
/// </summary>
public sealed class GamepadVirtualMouseCursor : MonoBehaviour
{
    private const float ActivityThreshold = 0.01f;
    private const float GamepadAxisChangeThreshold = 0.0001f;

    [Tooltip("カーソル速度と見た目を管理する設定")]
    [SerializeField] private GameUICursorSettings settings;

    [Tooltip("画面上で移動する仮想カーソルの RectTransform")]
    [SerializeField] private RectTransform cursorTransform;

    [Tooltip("仮想カーソルの Image")]
    [SerializeField] private Image cursorImage;

    private Mouse physicalMouse;
    private Mouse virtualMouse;
    private Vector2 cursorPosition;
    private Vector2 previousDpad;
    private Vector2 previousLeftStick;
    private Vector2 previousRightStick;
    private float previousLeftTrigger;
    private float previousRightTrigger;
    private PointerOwner pointerOwner;
    private bool hasPublishedVirtualPosition;

    /// <summary>
    /// 現在のカーソル位置を画面座標で返す
    /// </summary>
    public Vector2 CursorPosition => cursorPosition;

    private enum PointerOwner
    {
        PhysicalMouse,
        Gamepad
    }

    /// <summary>
    /// 仮想マウスを初期化し、設定済みのカーソル表示を適用する
    /// </summary>
    private void OnEnable()
    {
        physicalMouse = Mouse.current;
        virtualMouse = InputSystem.AddDevice<Mouse>("Gamepad Virtual Mouse");
        pointerOwner = PointerOwner.PhysicalMouse;
        cursorPosition = GetPhysicalMousePosition();
        previousDpad = Vector2.zero;
        previousLeftStick = Vector2.zero;
        previousRightStick = Vector2.zero;
        previousLeftTrigger = 0f;
        previousRightTrigger = 0f;
        hasPublishedVirtualPosition = false;
        PublishVirtualPosition(Vector2.zero);

        if (settings != null && cursorTransform != null)
        {
            cursorTransform.sizeDelta = Vector2.one * settings.CursorSize;
        }

        if (settings != null && cursorImage != null)
        {
            cursorImage.color = settings.CursorColor;
        }

        SetVirtualCursorVisible(false);
    }

    /// <summary>
    /// Input System に追加した仮想マウスを解除する
    /// </summary>
    private void OnDisable()
    {
        SetVirtualCursorVisible(false);
        PublishVirtualPosition(Vector2.zero);

        if (virtualMouse != null && virtualMouse.added)
        {
            InputSystem.RemoveDevice(virtualMouse);
        }

        virtualMouse = null;
    }

    /// <summary>
    /// 入力状況に応じてホバーを制御するポインターの所有者を更新する
    /// </summary>
    /// <param name="cursorAllowed">現在のゲーム状態でカーソル操作が許可されているか</param>
    /// <returns>ゲームパッドがポインターを所有しているか</returns>
    public bool UpdatePointer(bool cursorAllowed)
    {
        bool wasGamepadOwner = pointerOwner == PointerOwner.Gamepad;

        // ゲームプレイ中の視点操作をUIカーソルの操作として扱わない
        if (cursorAllowed
            && Cursor.lockState != CursorLockMode.Locked
            && HasPhysicalMouseActivity())
        {
            pointerOwner = PointerOwner.PhysicalMouse;
        }

        Gamepad gamepad = Gamepad.current;
        // 同じフレームに両方の入力があった場合はゲームパッドを優先する
        if (gamepad != null && HasGamepadActivity(gamepad))
        {
            pointerOwner = PointerOwner.Gamepad;
        }
        else if (gamepad == null)
        {
            pointerOwner = PointerOwner.PhysicalMouse;
            previousDpad = Vector2.zero;
            previousLeftStick = Vector2.zero;
            previousRightStick = Vector2.zero;
            previousLeftTrigger = 0f;
            previousRightTrigger = 0f;
        }

        bool isGamepadOwner = pointerOwner == PointerOwner.Gamepad;
        bool showVirtualCursor = isGamepadOwner && cursorAllowed;
        SetVirtualCursorVisible(showVirtualCursor);

        if (!isGamepadOwner)
        {
            if (wasGamepadOwner)
            {
                cursorPosition = GetPhysicalMousePosition();
                PublishVirtualPosition(Vector2.zero);
                hasPublishedVirtualPosition = false;
            }

            return false;
        }

        if (!wasGamepadOwner)
        {
            cursorPosition = GetPhysicalMousePosition();
            hasPublishedVirtualPosition = false;
        }

        if (!cursorAllowed || gamepad == null)
        {
            return true;
        }

        Vector2 dpad = gamepad.dpad.ReadValue();
        Vector2 stick = gamepad.leftStick.ReadValue();
        Vector2 movement = (dpad * GetDpadSpeed() + stick * GetStickSpeed()) * Time.unscaledDeltaTime;

        if (!hasPublishedVirtualPosition || movement.sqrMagnitude > 0f)
        {
            cursorPosition = ClampToScreen(cursorPosition + movement);
            PublishVirtualPosition(movement);
            hasPublishedVirtualPosition = true;
        }

        if (cursorTransform != null)
        {
            cursorTransform.position = cursorPosition;
        }

        return true;
    }

    /// <summary>
    /// マウスの移動やボタン操作があったかを調べる
    /// </summary>
    private bool HasPhysicalMouseActivity()
    {
        if (physicalMouse == null || !physicalMouse.added)
        {
            return false;
        }

        return physicalMouse.delta.ReadValue().sqrMagnitude > ActivityThreshold
            || physicalMouse.scroll.ReadValue().sqrMagnitude > ActivityThreshold
            || WasPressedOrReleased(physicalMouse.leftButton)
            || WasPressedOrReleased(physicalMouse.rightButton)
            || WasPressedOrReleased(physicalMouse.middleButton);
    }

    /// <summary>
    /// ゲームパッドの操作があったかを調べる
    /// 軸を倒し続けた状態だけでは所有権を切り替えない
    /// </summary>
    private bool HasGamepadActivity(Gamepad gamepad)
    {
        Vector2 dpad = gamepad.dpad.ReadValue();
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        float leftTrigger = gamepad.leftTrigger.ReadValue();
        float rightTrigger = gamepad.rightTrigger.ReadValue();

        bool axisChanged = (dpad - previousDpad).sqrMagnitude > GamepadAxisChangeThreshold
            || leftStick.sqrMagnitude > 0.0225f
                && (leftStick - previousLeftStick).sqrMagnitude > GamepadAxisChangeThreshold
            || rightStick.sqrMagnitude > 0.0225f
                && (rightStick - previousRightStick).sqrMagnitude > GamepadAxisChangeThreshold
            || Mathf.Abs(leftTrigger - previousLeftTrigger) > ActivityThreshold
            || Mathf.Abs(rightTrigger - previousRightTrigger) > ActivityThreshold;

        previousDpad = dpad;
        previousLeftStick = leftStick;
        previousRightStick = rightStick;
        previousLeftTrigger = leftTrigger;
        previousRightTrigger = rightTrigger;

        return axisChanged
            || WasPressedOrReleased(gamepad.buttonSouth)
            || WasPressedOrReleased(gamepad.buttonEast)
            || WasPressedOrReleased(gamepad.buttonWest)
            || WasPressedOrReleased(gamepad.buttonNorth)
            || WasPressedOrReleased(gamepad.leftShoulder)
            || WasPressedOrReleased(gamepad.rightShoulder)
            || WasPressedOrReleased(gamepad.leftStickButton)
            || WasPressedOrReleased(gamepad.rightStickButton)
            || WasPressedOrReleased(gamepad.startButton)
            || WasPressedOrReleased(gamepad.selectButton);
    }

    /// <summary>
    /// ボタンが押されたか、離されたかを調べる
    /// </summary>
    private static bool WasPressedOrReleased(ButtonControl button)
    {
        return button.wasPressedThisFrame || button.wasReleasedThisFrame;
    }

    /// <summary>
    /// 現在の物理マウス位置を画面内の座標にする
    /// </summary>
    private Vector2 GetPhysicalMousePosition()
    {
        Vector2 position = physicalMouse != null && physicalMouse.added
            ? physicalMouse.position.ReadValue()
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        return ClampToScreen(position);
    }

    /// <summary>
    /// カーソル位置を画面内に収める
    /// </summary>
    private static Vector2 ClampToScreen(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, 0f, Screen.width),
            Mathf.Clamp(position.y, 0f, Screen.height));
    }

    /// <summary>
    /// 仮想カーソル設定がない場合に使う十字キー速度を返す
    /// </summary>
    private float GetDpadSpeed()
    {
        return settings != null ? settings.DpadSpeed : 480f;
    }

    /// <summary>
    /// 仮想カーソル設定がない場合に使うスティック速度を返す
    /// </summary>
    private float GetStickSpeed()
    {
        return settings != null ? settings.StickSpeed : 960f;
    }

    /// <summary>
    /// 仮想マウスの位置を Input System に通知する
    /// </summary>
    private void PublishVirtualPosition(Vector2 movement)
    {
        if (virtualMouse == null || !virtualMouse.added)
        {
            return;
        }

        MouseState state = new MouseState
        {
            position = cursorPosition,
            delta = movement
        };

        InputState.Change(virtualMouse, state);
    }

    /// <summary>
    /// 仮想カーソルの表示状態を必要な場合だけ変更する
    /// </summary>
    private void SetVirtualCursorVisible(bool isVisible)
    {
        if (cursorTransform != null && cursorTransform.gameObject.activeSelf != isVisible)
        {
            cursorTransform.gameObject.SetActive(isVisible);
        }
    }
}
