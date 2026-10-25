using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クリア画面とゲームオーバー画面の表示演出を管理する
/// </summary>
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

    [Tooltip("クリア見出しの文字色")]
    [SerializeField] private Color clearColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("ゲームオーバー見出しの文字色")]
    [SerializeField] private Color gameOverColor = new Color(1f, 0.2f, 0.2f);

    [Tooltip("経過時間の表示形式")]
    [SerializeField] private TimeDisplayFormat elapsedTimeDisplayFormat = TimeDisplayFormat.MinutesSeconds;

    [Header("表示演出（秒・ゲーム停止中も再生）")]
    [SerializeField, Min(0f)] private float fadeToBlackDuration = 0.8f;
    [SerializeField, Min(0f)] private float blackHoldDuration = 0.15f;
    [SerializeField, Min(0f)] private float resultBackgroundFadeDuration = 0.4f;
    [SerializeField, Min(0f)] private float resultRevealDuration = 0.55f;
    [SerializeField, Min(0f)] private float scoreStartDelay = 0.25f;
    [SerializeField, Min(0f)] private float scoreRevealDuration = 0.28f;
    [SerializeField, Min(0f)] private float scoreInterval = 0.12f;
    [Tooltip("スコアが下から移動する距離（UI座標）")]
    [SerializeField, Min(0f)] private float scoreSlideDistance = 24f;
    [Tooltip("見出しの表示開始時の拡大率")]
    [SerializeField, Min(1f)] private float resultStartScale = 1.15f;

    private RevealTarget resultRevealTarget;
    private RevealTarget[] scoreRevealTargets;
    private Coroutine revealCoroutine;
    private CanvasGroup blackFadeGroup;

    private void Start()
    {
        gameEndCanvas.gameObject.SetActive(false);
    }

    public void Show(GameResult result, float elapsedSeconds, GameStats stats)
    {
        StopReveal();
        InitializeRevealTargets();
        hudCanvas.enabled = false;

        SetText(resultText, result == GameResult.Clear ? clearLabel : gameOverLabel);
        if (resultText != null)
            resultText.color = result == GameResult.Clear ? clearColor : gameOverColor;

        SetText(elapsedTimeText, TimeFormatter.Format(elapsedSeconds, elapsedTimeDisplayFormat));

        SetText(killCountText, stats.KillCount.ToString());
        SetText(fireCountText, stats.FireCount.ToString());
        SetText(totalDamageText, stats.TotalDamage.ToString());
        SetText(hitsPerFireText, stats.HitsPerFire.ToString("0.00"));

        // Canvasを表示する前に隠し、最初のフレームで全スコアが見えるのを防ぐ。
        resultRevealTarget?.Apply(0f, 0f, resultStartScale);
        foreach (RevealTarget target in scoreRevealTargets)
            target?.Apply(0f, scoreSlideDistance, 1f);

        gameEndCanvas.gameObject.SetActive(false);
        revealCoroutine = StartCoroutine(RevealSequence());
    }

    private void InitializeRevealTargets()
    {
        if (scoreRevealTargets != null)
            return;

        resultRevealTarget = CreateRevealTarget(resultText);
        // GameEndCanvasの上から順に並ぶスコア。
        scoreRevealTargets = new[]
        {
            CreateRevealTarget(elapsedTimeText),
            CreateRevealTarget(killCountText),
            CreateRevealTarget(fireCountText),
            CreateRevealTarget(totalDamageText),
            CreateRevealTarget(hitsPerFireText)
        };
    }

    private static RevealTarget CreateRevealTarget(TMP_Text text)
    {
        return text != null ? new RevealTarget(text) : null;
    }

    private IEnumerator RevealSequence()
    {
        yield return FadeToBlack();
        if (blackHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(blackHoldDuration);

        gameEndCanvas.gameObject.SetActive(true);
        float backgroundElapsed = 0f;
        while (backgroundElapsed < resultBackgroundFadeDuration)
        {
            blackFadeGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, backgroundElapsed / resultBackgroundFadeDuration);
            yield return null;
            backgroundElapsed += Time.unscaledDeltaTime;
        }
        blackFadeGroup.gameObject.SetActive(false);
        yield return Reveal(resultRevealTarget, resultRevealDuration, 0f, resultStartScale);
        if (scoreStartDelay > 0f)
            yield return new WaitForSecondsRealtime(scoreStartDelay);

        for (int i = 0; i < scoreRevealTargets.Length; i++)
        {
            if (scoreRevealTargets[i] == null)
                continue;

            yield return Reveal(scoreRevealTargets[i], scoreRevealDuration, scoreSlideDistance, 1f);
            if (i < scoreRevealTargets.Length - 1 && scoreInterval > 0f)
                yield return new WaitForSecondsRealtime(scoreInterval);
        }

        revealCoroutine = null;
    }

    private IEnumerator FadeToBlack()
    {
        if (blackFadeGroup == null)
        {
            // 終了画面を隠したまま、全画面と入力を覆う専用Canvasで暗転する。
            var overlay = new GameObject("GameEndBlackFade", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            overlay.transform.SetParent(gameEndCanvas.transform.parent, false);
            Canvas canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = Mathf.Max(gameEndCanvas.sortingOrder, hudCanvas.sortingOrder) + 1;
            canvas.targetDisplay = gameEndCanvas.targetDisplay;
            blackFadeGroup = overlay.GetComponent<CanvasGroup>();

            var background = new GameObject("Black", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(overlay.transform, false);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = Color.black;
        }

        blackFadeGroup.alpha = 0f;
        blackFadeGroup.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeToBlackDuration)
        {
            blackFadeGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeToBlackDuration);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        blackFadeGroup.alpha = 1f;
    }

    private static IEnumerator Reveal(RevealTarget target, float duration, float slideDistance, float startScale)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.Apply(elapsed / duration, slideDistance, startScale);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        target.Apply(1f, slideDistance, startScale);
    }

    private void OnDisable()
    {
        StopReveal();
    }

    private void StopReveal()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        if (blackFadeGroup != null)
            blackFadeGroup.gameObject.SetActive(false);

        resultRevealTarget?.Restore();
        if (scoreRevealTargets != null)
        {
            foreach (RevealTarget target in scoreRevealTargets)
                target?.Restore();
        }
    }

    /// <summary>
    /// 元の位置・大きさ・透明度を保存し、再表示時にも同じ状態へ戻す。
    /// </summary>
    private sealed class RevealTarget
    {
        private readonly RectTransform rect;
        private readonly CanvasGroup group;
        private readonly Vector2 position;
        private readonly Vector3 scale;
        private readonly float alpha;

        public RevealTarget(TMP_Text text)
        {
            rect = text.rectTransform;
            position = rect.anchoredPosition;
            scale = rect.localScale;
            group = text.GetComponent<CanvasGroup>();
            if (group == null)
                group = text.gameObject.AddComponent<CanvasGroup>();
            alpha = group.alpha;
        }

        public void Apply(float progress, float slideDistance, float startScale)
        {
            progress = Mathf.Clamp01(progress);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            rect.anchoredPosition = position + Vector2.down * (slideDistance * (1f - eased));
            rect.localScale = scale * Mathf.Lerp(startScale, 1f, eased);
            group.alpha = alpha * progress;
        }

        public void Restore()
        {
            rect.anchoredPosition = position;
            rect.localScale = scale;
            group.alpha = alpha;
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
