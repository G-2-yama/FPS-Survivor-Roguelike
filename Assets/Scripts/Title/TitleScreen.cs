using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面のパネル遷移とカーソル入力を管理する
/// </summary>
public class TitleScreen : MonoBehaviour
{
    [Header("パネル")]
    [Tooltip("ゲームスタート・設定・ゲーム終了を並べるパネル")]
    [SerializeField] private GameObject titlePanel;

    [Tooltip("モード選択のパネル")]
    [SerializeField] private GameObject modeSelectPanel;

    [Tooltip("初期武器とシンクロを選ぶパネル")]
    [SerializeField] private GameObject loadoutPanel;

    [Tooltip("音量と視点感度を変えるパネル")]
    [SerializeField] private GameObject settingsPanel;

    [Header("タイトル")]
    [Tooltip("押すとモード選択へ進むボタン")]
    [SerializeField] private Button gameStartButton;

    [Tooltip("押すと設定へ進むボタン")]
    [SerializeField] private Button settingsButton;

    [Tooltip("押すとゲームを終了するボタン")]
    [SerializeField] private Button quitButton;

    [Tooltip("ゲーム終了ボタンを表示するか 出展時など来場者に押させたくない場合はオフにします")]
    [SerializeField] private bool showQuitButton = true;

    [Header("モード選択")]
    [Tooltip("モードが選ばれたらアイテム選択へ進みます")]
    [SerializeField] private ModeSelectMenu modeSelectMenu;

    [Tooltip("押すとタイトルへ戻るボタン")]
    [SerializeField] private Button modeSelectBackButton;

    [Header("アイテム選択")]
    [Tooltip("押すとモード選択へ戻るボタン")]
    [SerializeField] private Button loadoutBackButton;

    [Header("設定")]
    [Tooltip("戻る操作でタイトルへ戻ります")]
    [SerializeField] private SettingsMenu settingsMenu;

    [Header("演出")]
    [Tooltip("左のキービジュアル")]
    [SerializeField] private TitleKeyVisual keyVisual;

    [Tooltip("画面切り替えのグリッチと明滅")]
    [SerializeField] private TitleGlitchFx glitchFx;

    [Tooltip("起動時に動かす要素 開始の遅れは各要素で設定します")]
    [SerializeField] private List<TitleSlideIn> introEffects = new List<TitleSlideIn>();

    [Tooltip("タイトル画面でゲームパッドの仮想カーソルを制御する")]
    [SerializeField] private GamepadVirtualMouseCursor gamepadVirtualMouseCursor;

    [Tooltip("グリッチが始まってからパネルを差し替えるまでの時間（秒）")]
    [SerializeField] private float panelSwapDelay = 0.12f;

    private GameObject currentPanel;
    private Coroutine swapRoutine;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameStartSettings.IsTutorial = false;

        quitButton.gameObject.SetActive(showQuitButton);

        gameStartButton.onClick.AddListener(() => ShowPanel(modeSelectPanel));
        settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        quitButton.onClick.AddListener(Quit);
        modeSelectMenu.OnModeSelected += () => ShowPanel(loadoutPanel);
        modeSelectBackButton.onClick.AddListener(() => ShowPanel(titlePanel));
        loadoutBackButton.onClick.AddListener(() => ShowPanel(modeSelectPanel));
        settingsMenu.BackRequested += () => ShowPanel(titlePanel);

        keyVisual.SetTitleMode(true, true);
        ActivatePanel(titlePanel);
        foreach (var effect in introEffects)
            effect.Play();
    }

    /// <summary>
    /// タイトル画面の入力元に合わせて仮想カーソルとOSカーソルを切り替える
    /// </summary>
    private void Update()
    {
        if (gamepadVirtualMouseCursor != null)
            gamepadVirtualMouseCursor.SuppressVirtualCursor = currentPanel == settingsPanel;

        bool gamepadOwnsPointer = gamepadVirtualMouseCursor != null
            && gamepadVirtualMouseCursor.UpdatePointer(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = !gamepadOwnsPointer;
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == currentPanel)
            return;

        currentPanel = panel;
        keyVisual.SetTitleMode(panel == titlePanel, false);
        keyVisual.Glitch(glitchFx.Duration);
        glitchFx.Play();

        if (swapRoutine != null)
            StopCoroutine(swapRoutine);
        swapRoutine = StartCoroutine(SwapPanel(panel));
    }

    private IEnumerator SwapPanel(GameObject panel)
    {
        yield return new WaitForSecondsRealtime(panelSwapDelay);
        ActivatePanel(panel);
        swapRoutine = null;
    }

    private void ActivatePanel(GameObject panel)
    {
        currentPanel = panel;
        titlePanel.SetActive(panel == titlePanel);
        modeSelectPanel.SetActive(panel == modeSelectPanel);
        loadoutPanel.SetActive(panel == loadoutPanel);
        settingsPanel.SetActive(panel == settingsPanel);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
