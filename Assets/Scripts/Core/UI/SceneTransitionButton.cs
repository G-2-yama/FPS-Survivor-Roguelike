using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour
{
    [Tooltip("押したときに遷移するシーン 未設定の場合は現在のシーンを読み込み直します")]
    [SceneReference]
    [SerializeField] private string targetScenePath;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(Transition);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Transition);
    }

    private void Transition()
    {
        if (string.IsNullOrEmpty(targetScenePath))
        {
            SceneLoader.ReloadActiveScene();
            return;
        }

        SceneLoader.Load(targetScenePath);
    }
}
