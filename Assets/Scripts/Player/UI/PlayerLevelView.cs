using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの現在レベルをUIテキストへ反映するビュー
/// </summary>
public class PlayerLevelView : MonoBehaviour
{
    /// <summary>
    /// 表示対象のプレイヤーレベルモデル
    /// </summary>
    [SerializeField, PlayerSettingLabel("Player"), Tooltip("経験値とレベルを表示するプレイヤー")] private Player player;

    /// <summary>
    /// 現在レベルを表示するテキスト
    /// </summary>
    [SerializeField, PlayerSettingLabel("Current Level Text"), Tooltip("現在レベルを表示するテキスト")] private TMP_Text currentLevelText;

    [SerializeField, PlayerSettingLabel("Current Level Bar"), Tooltip("次のレベルアップまでの残余経験値を表示するImage")] private Image currentLevelBar;

    /// <summary>
    /// レベル変更通知を購読する
    /// </summary>
    private void Start()
    {
        player.OnLevelUp += UpdateLevelText;
        player.OnExpGained += UpdateLevelBar;
        UpdateLevelText();
        UpdateLevelBar(0);
    }

    /// <summary>
    /// レベル表示を現在値で更新する
    /// </summary>
    private void UpdateLevelText()
    {
        int level = player.Level;
        currentLevelText.text = $"Lv.{level}";
        currentLevelBar.fillAmount = (float)player.Exp / player.LevelUpRequiredExp;
    }

    /// <summary>確定後の残余経験値と必要経験値からゲージを更新する</summary>
    private void UpdateLevelBar(float expGained)
    {
        currentLevelBar.fillAmount = (float)player.Exp / player.LevelUpRequiredExp;
    }

    /// <summary>
    /// レベル変更通知の購読を解除する
    /// </summary>
    private void OnDestroy()
    {
        player.OnLevelUp -= UpdateLevelText;
        player.OnExpGained -= UpdateLevelBar;
    }
}
