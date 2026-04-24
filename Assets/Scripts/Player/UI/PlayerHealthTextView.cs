using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの現在HPをUIテキストへ反映するビュー
/// </summary>
public class PlayerHealthTextView : MonoBehaviour
{
    /// <summary>
    /// 表示対象のプレイヤー体力モデル
    /// </summary>
    [SerializeField] private Player player;

    /// <summary>
    /// 現在HPと最大HPを表示するテキスト
    /// </summary>
    [SerializeField] private Text currentHealthText;

    /// <summary>
    /// HP変更通知を購読する
    /// </summary>
    private void Start()
    {
        player.Health.OnHealthChanged += UpdateHealthText;
    }

    /// <summary>
    /// HP表示を現在値と最大値で更新する
    /// </summary>
    /// <param name="current">現在HP</param>
    /// <param name="max">最大HP</param>
    private void UpdateHealthText(int current, int max)
    {
        currentHealthText.text = $"{current} / {max}";
    }

    /// <summary>
    /// HP変更通知の購読を解除する
    /// </summary>
    private void OnDestroy()
    {
        player.Health.OnHealthChanged -= UpdateHealthText;
    }
}
