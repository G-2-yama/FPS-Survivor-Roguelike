using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TutorialMenu : MonoBehaviour
{
    [Tooltip("開いている間だけ表示するメニューの画面")]
    [SerializeField] private GameObject panel;

    [Tooltip("メニューの項目を上下左右で操作する SettingsMenu 戻る操作でメニューを閉じます")]
    [SerializeField] private SettingsMenu menu;

    private GameManager gameManager;
    private GamepadVirtualMouseCursor virtualCursor;
    private TutorialMenuState menuState;

    // 最後に開閉したフレーム。SettingsMenu.Update も同じ Esc で BackRequested を出すので、
    // 同じフレームの2回目の開閉は無視する。外すと開いた直後に閉じることがある。
    private int toggledFrame = -1;

    private GameStateMachine StateMachine => gameManager != null ? gameManager.GameStateMachine : null;

    private void Awake()
    {
        panel.SetActive(false);
        menu.BackRequested += Close;
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        virtualCursor = FindFirstObjectByType<GamepadVirtualMouseCursor>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager が見つからないため、チュートリアルのメニューを開けません");
            return;
        }

        menuState = new TutorialMenuState(new GameController(gameManager));
    }

    private void Update()
    {
        var stateMachine = StateMachine;
        if (menuState == null || stateMachine == null)
            return;

        if (panel.activeSelf && stateMachine.CurrentState != menuState)
            Hide();

        if (toggledFrame == Time.frameCount || !WasTogglePressed())
            return;

        if (stateMachine.CurrentState == menuState)
            Close();
        else if (stateMachine.CurrentState == stateMachine.PlayingState)
            Open();
    }

    private void Open()
    {
        toggledFrame = Time.frameCount;
        StateMachine.ChangeState(menuState);
        SetVirtualCursorSuppressed(true);
        panel.SetActive(true);
    }

    private void Close()
    {
        var stateMachine = StateMachine;
        if (toggledFrame == Time.frameCount || stateMachine == null || stateMachine.CurrentState != menuState)
            return;

        toggledFrame = Time.frameCount;
        Hide();
        stateMachine.ChangePlayingState();
    }

    private void Hide()
    {
        panel.SetActive(false);
        SetVirtualCursorSuppressed(false);
    }

    private void SetVirtualCursorSuppressed(bool suppressed)
    {
        if (virtualCursor != null)
            virtualCursor.SuppressVirtualCursor = suppressed;
    }

    public static string ToggleDisplayName(bool gamepad)
    {
        ButtonControl control = gamepad ? GamepadToggle() : KeyboardToggle();
        if (control != null)
            return control.displayName;

        return gamepad ? "Select" : "Esc";
    }

    private static bool WasTogglePressed()
    {
        ButtonControl keyboardToggle = KeyboardToggle();
        ButtonControl gamepadToggle = GamepadToggle();
        return keyboardToggle != null && keyboardToggle.wasPressedThisFrame
            || gamepadToggle != null && gamepadToggle.wasPressedThisFrame;
    }

    private static ButtonControl KeyboardToggle() => Keyboard.current?.escapeKey;

    private static ButtonControl GamepadToggle() => Gamepad.current?.selectButton;
}
