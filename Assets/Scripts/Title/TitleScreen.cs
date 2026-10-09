using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("パネル")]
    [Tooltip("ゲームスタート・設定・ゲーム終了を並べるパネル")]
    [SerializeField] private GameObject titlePanel;

    [Tooltip("モード選択のパネル")]
    [SerializeField] private GameObject modeSelectPanel;

    [Tooltip("初期武器とシンクロを選ぶパネル")]
    [SerializeField] private GameObject loadoutPanel;

    [Header("タイトル")]
    [Tooltip("押すとモード選択へ進むボタン")]
    [SerializeField] private Button gameStartButton;

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

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        quitButton.gameObject.SetActive(showQuitButton);

        gameStartButton.onClick.AddListener(() => ShowPanel(modeSelectPanel));
        quitButton.onClick.AddListener(Quit);
        modeSelectMenu.OnModeSelected += () => ShowPanel(loadoutPanel);
        modeSelectBackButton.onClick.AddListener(() => ShowPanel(titlePanel));
        loadoutBackButton.onClick.AddListener(() => ShowPanel(modeSelectPanel));

        ShowPanel(titlePanel);
    }

    private void ShowPanel(GameObject panel)
    {
        titlePanel.SetActive(panel == titlePanel);
        modeSelectPanel.SetActive(panel == modeSelectPanel);
        loadoutPanel.SetActive(panel == loadoutPanel);
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
