using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの現在レベルをUIテキストへ反映するビュー
/// </summary>
public class PlayerLevelView : MonoBehaviour
{
    /// <summary>
    /// 表示対象のプレイヤーレベルモデル
    /// </summary>
    [SerializeField] private Player player;

    /// <summary>
    /// 現在レベルを表示するテキスト
    /// </summary>
    [SerializeField] private Text currentLevelText;

    /// <summary>
    /// レベル変更通知を購読する
    /// </summary>
    private void Start()
    {
        player.OnLevelUp += UpdateLevelText;
    }

    /// <summary>
    /// レベル表示を現在値で更新する
    /// </summary>
    private void UpdateLevelText()
    {
        int level = player.Level;
        currentLevelText.text = $"Lv.{level}";
    }

    /// <summary>
    /// レベル変更通知の購読を解除する
    /// </summary>
    private void OnDestroy()
    {
        player.OnLevelUp -= UpdateLevelText;
    }
}
