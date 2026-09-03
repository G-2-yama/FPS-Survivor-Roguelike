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

    [SerializeField] private Image currentHealthBar;
    [SerializeField] private Image characterImage;
    [SerializeField] private Sprite[] characterSprites;

    /// <summary>
    /// HP変更通知を購読する
    /// </summary>
    private void Start()
    {
        player.Health.OnHealthChanged += UpdateHealthText;
        UpdateHealthText(player.Health.CurrentHP, player.Health.MaxHP);
    }

    /// <summary>
    /// HP表示を現在値と最大値で更新する
    /// </summary>
    /// <param name="current">現在HP</param>
    /// <param name="max">最大HP</param>
    private void UpdateHealthText(int current, int max)
    {
        currentHealthText.text = $"{current}/{max}";
        currentHealthBar.fillAmount = (float)current / max;
        UpdateCharacterSprite();
    }

    /// <summary>
    /// HP変更通知の購読を解除する
    /// </summary>
    private void OnDestroy()
    {
        player.Health.OnHealthChanged -= UpdateHealthText;
    }

    /// <summary>
    /// 現在のHPに応じてキャラクター画像を更新する
    /// </summary>
    private void UpdateCharacterSprite()
    {
        float healthPercentage = (float)player.Health.CurrentHP / player.Health.MaxHP;

        if (healthPercentage > 0.75f)
        {
            characterImage.sprite = characterSprites[0];
        }
        else if (healthPercentage > 0.5f)
        {
            characterImage.sprite = characterSprites[1];
        }
        else if (healthPercentage > 0.25f)
        {
            characterImage.sprite = characterSprites[2];
        }
        else
        {
            characterImage.sprite = characterSprites[3];
        }
    }
}
