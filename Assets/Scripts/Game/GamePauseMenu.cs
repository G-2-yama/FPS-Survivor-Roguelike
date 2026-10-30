using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>ゲーム中の設定、再開、リタイアを扱うポーズメニュー</summary>
public sealed class GamePauseMenu : MonoBehaviour
{
    [Tooltip("一時停止中だけ表示するパネル")]
    [SerializeField] private GameObject panel;

    [Tooltip("既存の感度・音量設定とボタンを操作するメニュー")]
    [SerializeField] private SettingsMenu menu;

    [Tooltip("ゲームオーバーとして今回のプレイを終了するボタン")]
    [SerializeField] private Button retireButton;

    private GameStateMachine stateMachine;
    private PauseMenuState pauseState;
    private GamepadVirtualMouseCursor virtualCursor;
    private int toggledFrame = -1;

    private void Awake()
    {
        panel.SetActive(false);
        menu.BackRequested += Close;
        retireButton.onClick.AddListener(Retire);
    }

    private void Start()
    {
        // GameManager.Start の初期化順に依存しないよう、状態の取得は Update で行う。
        virtualCursor = FindFirstObjectByType<GamepadVirtualMouseCursor>();
    }

    private void Update()
    {
        if (stateMachine == null)
        {
            var manager = FindFirstObjectByType<GameManager>();
            if (manager == null || manager.GameStateMachine == null)
                return;

            stateMachine = manager.GameStateMachine;
            pauseState = new PauseMenuState(new GameController(manager));
            stateMachine.OnStateChanged += HandleStateChanged;
        }

        if (GameStartSettings.IsTutorial)
            return;

        if (toggledFrame == Time.frameCount || !WasTogglePressed())
            return;

        if (stateMachine.CurrentState == pauseState)
            Close();
        else if (stateMachine.CurrentState == stateMachine.PlayingState)
        {
            toggledFrame = Time.frameCount;
            stateMachine.ChangeState(pauseState);
        }
    }

    private void Close()
    {
        // SettingsMenu も同じ Esc を処理するため、開いたフレームの再閉鎖を防ぐ。
        if (stateMachine == null || stateMachine.CurrentState != pauseState
            || toggledFrame == Time.frameCount)
            return;

        toggledFrame = Time.frameCount;
        stateMachine.ChangePlayingState();
    }

    private void Retire()
    {
        if (stateMachine != null && stateMachine.CurrentState == pauseState)
            stateMachine.ChangeEndState(GameResult.GameOver);
    }

    private void HandleStateChanged(GameState state)
    {
        bool open = state == pauseState;
        panel.SetActive(open);
        if (virtualCursor != null)
            virtualCursor.SuppressVirtualCursor = open;
    }

    private static bool WasTogglePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
            || Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;
    }

    private void OnDestroy()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChanged;
        if (virtualCursor != null)
            virtualCursor.SuppressVirtualCursor = false;
        if (menu != null)
            menu.BackRequested -= Close;
        if (retireButton != null)
            retireButton.onClick.RemoveListener(Retire);
    }
}
