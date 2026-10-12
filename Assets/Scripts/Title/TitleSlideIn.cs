using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TitleSlideIn : MonoBehaviour
{
    [Tooltip("動き始める位置 本来の位置からのずれ（px）")]
    [SerializeField] private Vector2 offset = new Vector2(80f, 0f);

    [Tooltip("動く時間（秒）")]
    [SerializeField] private float duration = 0.45f;

    [Tooltip("Play から動き始めるまでの時間（秒）")]
    [SerializeField] private float delay;

    [Tooltip("透明から表示させるか")]
    [SerializeField] private bool fade = true;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 home;
    private float startAt;
    private bool isPlaying;

    private void Awake()
    {
        rect = (RectTransform)transform;
        home = rect.anchoredPosition;

        if (fade && !TryGetComponent(out canvasGroup))
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Play()
    {
        startAt = TitleClock.Now + delay;
        isPlaying = true;
        Apply(0f);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        float t = (TitleClock.Now - startAt) / duration;
        if (t < 0f)
            return;

        if (t >= 1f)
        {
            Finish();
            return;
        }

        Apply(TitleEase.Out(t));
    }

    private void OnDisable()
    {
        if (isPlaying)
            Finish();
    }

    private void Finish()
    {
        isPlaying = false;
        Apply(1f);
    }

    private void Apply(float progress)
    {
        rect.anchoredPosition = home + offset * (1f - progress);

        if (canvasGroup != null)
            canvasGroup.alpha = progress;
    }
}
