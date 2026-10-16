using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// タイトル画面の二択ボタンの選択状態を表示する
/// </summary>
[RequireComponent(typeof(Button))]
public class TitleChoiceView : MonoBehaviour, IPointerEnterHandler
{
    [Tooltip("選ばれているときに色を変える対象")]
    [SerializeField] private List<TitleColorTarget> colors = new List<TitleColorTarget>();

    private Button button;

    public Button Button => button != null ? button : button = GetComponent<Button>();

    public void SetOn(bool isOn)
    {
        foreach (var target in colors)
            target.Apply(isOn ? 1f : 0f);
    }

    /// <summary>
    /// ポインターが選択肢に入ったときにゲームパッド決定の対象にする
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != gameObject)
            eventSystem.SetSelectedGameObject(gameObject);
    }
}
