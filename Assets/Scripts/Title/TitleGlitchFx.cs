using UnityEngine;

public class TitleGlitchFx : MonoBehaviour
{
    [Header("画面切り替え")]
    [Tooltip("切り替えのときに揺らす画面の中身")]
    [SerializeField] private RectTransform content;

    [Tooltip("切り替えのときに一瞬映る横帯の親")]
    [SerializeField] private RectTransform bars;

    [Tooltip("横帯の表示を切り替える CanvasGroup")]
    [SerializeField] private CanvasGroup barsGroup;

    [Tooltip("切り替えのグリッチの長さ（秒）")]
    [SerializeField] private float duration = 0.32f;

    public float Duration => duration;

    private Vector2 contentHome;
    private Vector2 barsHome;
    private float startedAt = float.NegativeInfinity;

    private void Awake()
    {
        contentHome = content.anchoredPosition;
        barsHome = bars.anchoredPosition;
        barsGroup.alpha = 0f;
    }

    public void Play()
    {
        startedAt = Time.unscaledTime;
    }

    private void Update()
    {
        UpdateTransition((Time.unscaledTime - startedAt) / duration);
    }

    private void UpdateTransition(float t)
    {
        if (t < 0f || t >= 1f)
        {
            content.anchoredPosition = contentHome;
            SetBars(0f, 0f, 1f);
            return;
        }

        content.anchoredPosition = contentHome + new Vector2(ShakeOffset(t), 0f);

        if (t < 0.25f)
            SetBars(0.9f, -0.12f, 1f);
        else if (t < 0.4f)
            SetBars(0f, 0f, 1f);
        else if (t < 0.6f)
            SetBars(0.8f, 0.08f, 0.5f);
        else if (t < 0.75f)
            SetBars(0f, 0f, 1f);
        else
            SetBars(0.6f, -0.04f, 1.6f);
    }

    private static float ShakeOffset(float t)
    {
        if (t < 0.25f)
            return -24f;
        if (t < 0.5f)
            return 18f;
        if (t < 0.75f)
            return -8f;
        return 6f;
    }

    private void SetBars(float alpha, float shift, float scaleY)
    {
        barsGroup.alpha = alpha;
        bars.anchoredPosition = barsHome + new Vector2(shift * bars.rect.width, 0f);
        bars.localScale = new Vector3(1f, scaleY, 1f);
    }
}
