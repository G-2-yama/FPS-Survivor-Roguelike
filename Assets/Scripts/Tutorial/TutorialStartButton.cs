using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TutorialStartButton : MonoBehaviour
{
    [Tooltip("チュートリアル用の設定で読み込む本編のシーン")]
    [SceneReference]
    [SerializeField] private string gameScenePath;

    [Tooltip("本編のシーンに加算で重ねるチュートリアルのシーン")]
    [SceneReference]
    [SerializeField] private string tutorialScenePath;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(StartTutorial);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(StartTutorial);
    }

    private void StartTutorial()
    {
        GameStartSettings.IsTutorial = true;
        SceneLoader.LoadWithAdditive(gameScenePath, tutorialScenePath);
    }
}
