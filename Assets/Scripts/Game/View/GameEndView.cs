using TMPro;
using UnityEngine;

public class GameEndView : MonoBehaviour
{
    [Header("プレイ中")]
    [Tooltip("ゲーム終了時に非表示にするHUD")]
    [SerializeField] private Canvas hudCanvas;

    [Header("終了画面")]
    [SerializeField] private Canvas gameEndCanvas;

    [Tooltip("クリア・ゲームオーバーの表記を出すテキスト")]
    [SerializeField] private TMP_Text resultText;

    [Tooltip("経過時間を出すテキスト")]
    [SerializeField] private TMP_Text elapsedTimeText;

    [Tooltip("倒した敵の数を出すテキスト")]
    [SerializeField] private TMP_Text killCountText;

    [Tooltip("攻撃回数を出すテキスト。弾1発ごとに数える")]
    [SerializeField] private TMP_Text fireCountText;

    [Tooltip("敵に与えた総ダメージを出すテキスト。敵の残りHPを超えた分は含めない")]
    [SerializeField] private TMP_Text totalDamageText;

    [Tooltip("攻撃1回あたりのヒット数を出すテキスト。範囲攻撃などで1を超えることがある")]
    [SerializeField] private TMP_Text hitsPerFireText;

    [Tooltip("時間切れまで生き残ったときの表記")]
    [SerializeField] private string clearLabel = "CLEAR";

    [Tooltip("死亡したときの表記")]
    [SerializeField] private string gameOverLabel = "GAME OVER";

    [Tooltip("経過時間の表示形式")]
    [SerializeField] private TimeDisplayFormat elapsedTimeDisplayFormat = TimeDisplayFormat.MinutesSeconds;

    private void Start()
    {
        gameEndCanvas.gameObject.SetActive(false);
    }

    public void Show(GameResult result, float elapsedSeconds, GameStats stats)
    {
        hudCanvas.enabled = false;

        SetText(resultText, result == GameResult.Clear ? clearLabel : gameOverLabel);

        SetText(elapsedTimeText, TimeFormatter.Format(elapsedSeconds, elapsedTimeDisplayFormat));

        SetText(killCountText, stats.KillCount.ToString());
        SetText(fireCountText, stats.FireCount.ToString());
        SetText(totalDamageText, stats.TotalDamage.ToString());
        SetText(hitsPerFireText, stats.HitsPerFire.ToString("0.00"));

        gameEndCanvas.gameObject.SetActive(true);
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
