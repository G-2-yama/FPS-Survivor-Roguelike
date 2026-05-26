using Unity.VisualScripting;
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

    [SerializeField] private Image currentLevelBar;

    [SerializeField] private ExpManager expmager;

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
        currentLevelBar.fillAmount = (float)player.Exp / expmager.LevelUpRequiredExp;
    }

    private void UpdateLevelBar(float expGained)
    {
        currentLevelBar.fillAmount = (float)player.Exp / expmager.LevelUpRequiredExp;
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
