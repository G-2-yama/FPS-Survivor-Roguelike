using System;
using UnityEngine;
using UnityEngine.UI;

public class GameModeOptionView : MonoBehaviour
{
    [Tooltip("押すとこのモードを選ぶボタン")]
    [SerializeField] private Button button;

    [Tooltip("モード名を表示するテキスト 未設定でも動作します")]
    [SerializeField] private Text nameText;

    public void Setup(GameModeData mode, Action onSelected)
    {
        if (nameText != null)
            nameText.text = mode.DisplayName;

        button.onClick.AddListener(() => onSelected());
    }
}
