using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ボタン選択時に指定シーンへ遷移する
/// </summary>
[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour, IPointerEnterHandler
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

    /// <summary>
    /// ポインターがボタンに入ったときにゲームパッド決定の対象にする
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != gameObject)
            eventSystem.SetSelectedGameObject(gameObject);
    }
}
