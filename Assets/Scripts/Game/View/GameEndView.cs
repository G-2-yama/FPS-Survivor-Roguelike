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

    public void Show(GameResult result, float elapsedSeconds)
    {
        hudCanvas.enabled = false;

        resultText.text = result == GameResult.Clear ? clearLabel : gameOverLabel;

        elapsedTimeText.text = TimeFormatter.Format(elapsedSeconds, elapsedTimeDisplayFormat);

        gameEndCanvas.gameObject.SetActive(true);
    }
}
