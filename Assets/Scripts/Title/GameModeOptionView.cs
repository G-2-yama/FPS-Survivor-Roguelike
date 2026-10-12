using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameModeOptionView : MonoBehaviour
{
    [Tooltip("押すとこのモードを選ぶボタン")]
    [SerializeField] private Button button;

    [Tooltip("モード名を表示するテキスト 未設定でも動作します")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("並び順の番号を表示するテキスト 未設定でも動作します")]
    [SerializeField] private TMP_Text numberText;

    [Tooltip("制限時間を表示するテキスト 未設定でも動作します")]
    [SerializeField] private TMP_Text timeText;

    [Tooltip("TIME LIMIT / NO LIMIT を表示するテキスト 制限時間の左に並べます 未設定でも動作します")]
    [SerializeField] private TMP_Text limitText;

    [Tooltip("エンドレスの ∞ の文字サイズ")]
    [SerializeField] private float endlessTimeFontSize = 110f;

    [Tooltip("制限時間と左の見出しの間隔（px）")]
    [SerializeField] private float limitSpacing = 24f;

    public void Setup(GameModeData mode, int number, Action onSelected)
    {
        if (nameText != null)
            nameText.text = mode.DisplayName;

        if (numberText != null)
            numberText.text = number.ToString("00");

        bool isEndless = mode.TimeLimitSeconds < 0f;
        if (timeText != null)
        {
            timeText.text = isEndless ? "∞" : FormatTime(mode.TimeLimitSeconds);
            if (isEndless)
                timeText.fontSize = endlessTimeFontSize;
        }

        if (limitText != null)
        {
            limitText.text = isEndless ? "NO LIMIT" : "TIME LIMIT";
            if (timeText != null)
                PlaceLimitText();
        }

        button.onClick.AddListener(() => onSelected());
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.RoundToInt(seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }

    private void PlaceLimitText()
    {
        RectTransform limitRect = limitText.rectTransform;
        float timeWidth = timeText.GetPreferredValues(timeText.text).x;
        limitRect.anchoredPosition = new Vector2(
            limitRect.anchoredPosition.x - timeWidth - limitSpacing,
            limitRect.anchoredPosition.y);
    }
}
