using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Tooltip("上下で選ぶ順に並べた項目 非表示の項目は飛ばします")]
    [SerializeField] private List<SettingsMenuItem> items = new List<SettingsMenuItem>();

    [Tooltip("押すとタイトルへ戻るボタン")]
    [SerializeField] private Button backButton;

    [Tooltip("押し続けたとき、繰り返し始めるまでの時間（秒）")]
    [SerializeField] private float repeatDelay = 0.4f;

    [Tooltip("押し続けたときの繰り返しの間隔（秒）")]
    [SerializeField] private float repeatInterval = 0.1f;

    [Tooltip("左スティックを方向の入力とみなす倒し具合")]
    [SerializeField, Range(0.1f, 1f)] private float stickThreshold = 0.5f;

    public event Action BackRequested;

    private readonly List<SettingsMenuItem> visibleItems = new List<SettingsMenuItem>();
    private SettingsMenuItem focused;
    private Vector2Int heldDirection;
    private float nextRepeatAt;

    private void Awake()
    {
        backButton.onClick.AddListener(RequestBack);
        foreach (var item in items)
            item.PointerEntered += Focus;
    }

    private void OnEnable()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        heldDirection = Vector2Int.zero;
        CollectVisibleItems();
        Focus(visibleItems.Count > 0 ? visibleItems[0] : null);
    }

    private void OnDisable()
    {
        Focus(null);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        if (WasCancelPressed(keyboard, gamepad))
        {
            RequestBack();
            return;
        }

        if (WasSubmitPressed(keyboard, gamepad) && focused != null)
            focused.Submit();

        var direction = ReadDirection(keyboard, gamepad);
        if (direction != heldDirection)
        {
            heldDirection = direction;
            if (direction != Vector2Int.zero)
            {
                Move(direction);
                nextRepeatAt = Time.unscaledTime + repeatDelay;
            }
        }
        else if (direction != Vector2Int.zero && Time.unscaledTime >= nextRepeatAt)
        {
            Move(direction);
            nextRepeatAt = Time.unscaledTime + repeatInterval;
        }
    }

    private void RequestBack()
    {
        BackRequested?.Invoke();
    }

    private void Move(Vector2Int direction)
    {
        if (direction.y != 0)
            FocusNext(-direction.y);
        else if (focused != null)
            focused.Step(direction.x);
    }

    private void FocusNext(int step)
    {
        CollectVisibleItems();
        if (visibleItems.Count == 0)
            return;

        int index = visibleItems.IndexOf(focused);
        int next = index < 0 ? 0 : (index + step + visibleItems.Count) % visibleItems.Count;
        Focus(visibleItems[next]);
    }

    private void Focus(SettingsMenuItem item)
    {
        if (focused == item)
            return;

        if (focused != null)
            focused.SetFocused(false);
        focused = item;
        if (focused != null)
            focused.SetFocused(true);
    }

    private void CollectVisibleItems()
    {
        visibleItems.Clear();
        foreach (var item in items)
        {
            if (item != null && item.gameObject.activeInHierarchy)
                visibleItems.Add(item);
        }
    }

    private Vector2Int ReadDirection(Keyboard keyboard, Gamepad gamepad)
    {
        Vector2 input = Vector2.zero;

        if (keyboard != null)
        {
            if (keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.leftArrowKey.isPressed) input.x -= 1f;
        }

        if (gamepad != null)
        {
            input += gamepad.dpad.ReadValue();
            Vector2 stick = gamepad.leftStick.ReadValue();
            if (stick.magnitude >= stickThreshold)
                input += stick;
        }

        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
            return input.y > 0.01f ? Vector2Int.up : input.y < -0.01f ? Vector2Int.down : Vector2Int.zero;
        return input.x > 0.01f ? Vector2Int.right : input.x < -0.01f ? Vector2Int.left : Vector2Int.zero;
    }

    private static bool WasCancelPressed(Keyboard keyboard, Gamepad gamepad)
    {
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame
            || gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
    }

    private static bool WasSubmitPressed(Keyboard keyboard, Gamepad gamepad)
    {
        return keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            || gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
    }
}
