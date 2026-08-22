using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ゲームパッドの方向入力を Input System の仮想マウスに変換するクラス
/// 既存のマウス対応 UI を、Selectable の設定なしで操作できるようにする
/// </summary>
public sealed class GamepadVirtualMouseCursor : MonoBehaviour
{
    private const float DpadSpeed = 480f;
    private const float StickSpeed = 960f;

    private Mouse virtualMouse;
    private Vector2 cursorPosition;
    private RectTransform cursorTransform;
    private PointerEventData pointerEventData;
    private GameObject pressedObject;
    private GameObject hoveredObject;
    private bool wasPrimaryPressed;
    private readonly List<RaycastResult> raycastResults = new();

    /// <summary>
    /// シーン読込後に仮想カーソルを1つだけ生成するメソッド
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIfNeeded()
    {
        if (FindFirstObjectByType<GamepadVirtualMouseCursor>() != null)
        {
            return;
        }

        GameObject cursorObject = new(nameof(GamepadVirtualMouseCursor));
        DontDestroyOnLoad(cursorObject);
        cursorObject.AddComponent<GamepadVirtualMouseCursor>();
    }

    /// <summary>
    /// Input System 上の仮想マウスと表示用カーソルを初期化するメソッド
    /// </summary>
    private void OnEnable()
    {
        virtualMouse = InputSystem.AddDevice<Mouse>("Gamepad Virtual Mouse");
        cursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        UpdateVirtualMouseState(Vector2.zero, false, false);
        CreateCursorVisual();
    }

    /// <summary>
    /// 登録済みの仮想マウスを Input System から解除するメソッド
    /// </summary>
    private void OnDisable()
    {
        if (virtualMouse != null && virtualMouse.added)
        {
            InputSystem.RemoveDevice(virtualMouse);
        }
    }

    /// <summary>
    /// カーソル表示中だけゲームパッド入力を仮想マウスへ反映するメソッド
    /// </summary>
    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        bool canControlCursor = gamepad != null && Cursor.visible;
        cursorTransform.gameObject.SetActive(canControlCursor);

        if (!canControlCursor)
        {
            UpdateVirtualMouseState(Vector2.zero, false, false);
            ProcessPrimaryClick(false, Vector2.zero);
            return;
        }

        Vector2 dpadInput = gamepad.dpad.ReadValue();
        Vector2 stickInput = gamepad.leftStick.ReadValue();
        Vector2 movement = (dpadInput * DpadSpeed + stickInput * StickSpeed) * Time.unscaledDeltaTime;

        cursorPosition = new Vector2(
            Mathf.Clamp(cursorPosition.x + movement.x, 0f, Screen.width),
            Mathf.Clamp(cursorPosition.y + movement.y, 0f, Screen.height));

        UpdateVirtualMouseState(
            movement,
            false,
            false);

        ProcessPrimaryClick(gamepad.buttonSouth.isPressed, movement);

        cursorTransform.position = cursorPosition;
    }

    /// <summary>
    /// 仮想カーソル位置の UI に対して、A / × のクリックイベントを送信するメソッド
    /// </summary>
    private void ProcessPrimaryClick(bool isPressed, Vector2 movement)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            wasPrimaryPressed = isPressed;
            return;
        }

        pointerEventData ??= new PointerEventData(eventSystem);
        pointerEventData.button = PointerEventData.InputButton.Left;
        pointerEventData.position = cursorPosition;
        pointerEventData.delta = movement;

        raycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, raycastResults);
        RaycastResult currentRaycast = raycastResults.Count > 0 ? raycastResults[0] : default;
        GameObject currentObject = currentRaycast.gameObject;
        pointerEventData.pointerCurrentRaycast = currentRaycast;
        UpdateHover(currentObject);

        if (isPressed && !wasPrimaryPressed)
        {
            pointerEventData.pressPosition = cursorPosition;
            pointerEventData.pointerPressRaycast = currentRaycast;
            pointerEventData.eligibleForClick = true;
            pressedObject = ExecuteEvents.ExecuteHierarchy(
                currentObject,
                pointerEventData,
                ExecuteEvents.pointerDownHandler);

            if (pressedObject == null)
            {
                pressedObject = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);
            }

            pointerEventData.pointerPress = pressedObject;
        }
        else if (!isPressed && wasPrimaryPressed)
        {
            if (pressedObject != null)
            {
                ExecuteEvents.Execute(pressedObject, pointerEventData, ExecuteEvents.pointerUpHandler);
            }

            GameObject clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);
            if (pressedObject != null && pressedObject == clickTarget && pointerEventData.eligibleForClick)
            {
                ExecuteEvents.Execute(pressedObject, pointerEventData, ExecuteEvents.pointerClickHandler);
            }

            pointerEventData.pointerPress = null;
            pressedObject = null;
            pointerEventData.eligibleForClick = false;
        }

        wasPrimaryPressed = isPressed;
    }

    /// <summary>
    /// 仮想カーソル下の UI にホバー開始・終了イベントを送信するメソッド
    /// </summary>
    private void UpdateHover(GameObject currentObject)
    {
        if (hoveredObject == currentObject)
        {
            return;
        }

        if (hoveredObject != null)
        {
            ExecuteEvents.ExecuteHierarchy(
                hoveredObject,
                pointerEventData,
                ExecuteEvents.pointerExitHandler);
        }

        hoveredObject = currentObject;

        if (hoveredObject != null)
        {
            ExecuteEvents.ExecuteHierarchy(
                hoveredObject,
                pointerEventData,
                ExecuteEvents.pointerEnterHandler);
        }
    }

    /// <summary>
    /// 位置、移動量、ボタン状態をまとめて仮想マウスへ反映するメソッド
    /// </summary>
    private void UpdateVirtualMouseState(Vector2 delta, bool leftPressed, bool rightPressed)
    {
        MouseState state = new MouseState
        {
            position = cursorPosition,
            delta = delta
        };

        state = state
            .WithButton(MouseButton.Left, leftPressed)
            .WithButton(MouseButton.Right, rightPressed);

        InputState.Change(virtualMouse, state);
    }

    /// <summary>
    /// 画面最前面に表示する仮想カーソルの見た目を生成するメソッド
    /// </summary>
    private void CreateCursorVisual()
    {
        GameObject canvasObject = new("Gamepad Virtual Cursor Canvas");
        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;
        canvasObject.AddComponent<CanvasScaler>();

        GameObject imageObject = new("Cursor");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f));
        image.color = Color.white;
        image.raycastTarget = false;

        cursorTransform = image.rectTransform;
        cursorTransform.sizeDelta = new Vector2(20f, 20f);
        cursorTransform.position = cursorPosition;
    }
}
