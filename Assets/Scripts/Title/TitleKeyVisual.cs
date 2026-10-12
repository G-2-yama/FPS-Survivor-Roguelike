using UnityEngine;
using UnityEngine.UI;

public class TitleKeyVisual : MonoBehaviour
{
    private readonly struct Pose
    {
        public readonly float Scale;
        public readonly float X;
        public readonly float Y;
        public readonly float Shade;
        public readonly bool Glitch;

        public Pose(float scale, float x, float y, float shade, bool glitch)
        {
            Scale = scale;
            X = x;
            Y = y;
            Shade = shade;
            Glitch = glitch;
        }

        public static Pose Lerp(Pose from, Pose to, float t) => new Pose(
            Mathf.Lerp(from.Scale, to.Scale, t),
            Mathf.Lerp(from.X, to.X, t),
            Mathf.Lerp(from.Y, to.Y, t),
            Mathf.Lerp(from.Shade, to.Shade, t),
            false);
    }

    private static readonly Vector2[] PanPath =
    {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
    };

    [Header("参照")]
    [Tooltip("サブ画面で左へずらすキービジュアル全体")]
    [SerializeField] private RectTransform body;

    [Tooltip("ズームとパンで動かす絵の親 左上を基準に拡大します")]
    [SerializeField] private RectTransform cam;

    [Tooltip("ズーム中に絵を暗くする幕")]
    [SerializeField] private Graphic shade;

    [Tooltip("サブ画面でキービジュアル全体を暗くする幕")]
    [SerializeField] private Graphic dim;

    [Tooltip("グリッチ中に左へずらす色ずれの絵")]
    [SerializeField] private RectTransform leftTint;

    [Tooltip("グリッチ中に右へずらす色ずれの絵")]
    [SerializeField] private RectTransform rightTint;

    [Tooltip("グリッチ中に横へずらす帯状の切り抜き 最初の子に絵を1枚持たせます")]
    [SerializeField] private RectTransform[] slices;

    [Header("タイトル画面の動き")]
    [Tooltip("ズームの倍率")]
    [SerializeField] private float zoom = 1.8f;

    [Tooltip("全体を見せて止まる時間（秒）")]
    [SerializeField] private float holdDuration = 4f;

    [Tooltip("ズームにかける時間（秒）")]
    [SerializeField] private float zoomInDuration = 0.5f;

    [Tooltip("パン1区間の時間（秒） 左上→右上→左下→右下の3区間です")]
    [SerializeField] private float legDuration = 7f;

    [Tooltip("ズーム中の暗さ")]
    [Range(0f, 1f)]
    [SerializeField] private float zoomedShade = 0.5f;

    [Tooltip("パンの終わりから全体に戻るカットの時間（秒）")]
    [SerializeField] private float cutDuration = 0.3f;

    [Tooltip("画面を切り替えたときに前の動きから移る時間（秒）")]
    [SerializeField] private float blendDuration = 0.8f;

    [Header("サブ画面")]
    [Tooltip("サブ画面で左へずらす量（px）")]
    [SerializeField] private float subOffset = 260f;

    [Tooltip("サブ画面での暗さ")]
    [Range(0f, 1f)]
    [SerializeField] private float subDim = 0.45f;

    [Tooltip("ずれと暗さの切り替えにかける時間（秒）")]
    [SerializeField] private float subTransitionDuration = 0.5f;

    [Header("グリッチ")]
    [Tooltip("色ずれと切り抜きを変える間隔（秒）")]
    [SerializeField] private float glitchInterval = 0.06f;

    private float size;
    private Vector2 bodyHome;
    private bool isTitleMode = true;
    private float modeStartedAt;
    private bool isBlending;
    private Pose blendFrom;
    private float blendStartedAt;
    private Pose current = new Pose(1f, 0f, 0f, 0f, false);
    private float glitchUntil = float.NegativeInfinity;
    private float lastScrambleAt = float.NegativeInfinity;
    private float jitter;
    private bool isGlitchVisible = true;
    private float subFrom;
    private float subTo;
    private float subStartedAt = float.NegativeInfinity;

    private void Awake()
    {
        size = cam.rect.width;
        bodyHome = body.anchoredPosition;
        modeStartedAt = TitleClock.Now;
    }

    public void SetTitleMode(bool isTitle, bool immediate)
    {
        float now = TitleClock.Now;
        blendFrom = current;
        isBlending = !immediate;
        blendStartedAt = now;
        isTitleMode = isTitle;
        modeStartedAt = now;

        subFrom = immediate ? (isTitle ? 0f : 1f) : SubValue(now);
        subTo = isTitle ? 0f : 1f;
        subStartedAt = now;
    }

    public void Glitch(float duration)
    {
        glitchUntil = Mathf.Max(glitchUntil, TitleClock.Now + duration);
    }

    private void Update()
    {
        float now = TitleClock.Now;
        float t = now - modeStartedAt;
        Pose pose = isTitleMode ? TitlePose(t) : DriftPose(t);

        if (isBlending)
        {
            float u = (now - blendStartedAt) / blendDuration;
            if (u >= 1f)
                isBlending = false;
            else
                pose = Pose.Lerp(blendFrom, pose, TitleEase.InOut(u));
        }

        current = pose;

        bool glitch = pose.Glitch || now < glitchUntil;
        if (glitch && now - lastScrambleAt > glitchInterval)
        {
            lastScrambleAt = now;
            Scramble();
        }

        SetGlitchVisible(glitch);
        cam.localScale = new Vector3(pose.Scale, pose.Scale, 1f);
        cam.anchoredPosition = new Vector2(-pose.X * pose.Scale + (glitch ? jitter : 0f), pose.Y * pose.Scale);
        SetAlpha(shade, pose.Shade);

        float sub = SubValue(now);
        body.anchoredPosition = bodyHome + new Vector2(-subOffset * sub, 0f);
        SetAlpha(dim, subDim * sub);
    }

    private float SubValue(float now)
    {
        float u = Mathf.Clamp01((now - subStartedAt) / subTransitionDuration);
        return Mathf.Lerp(subFrom, subTo, TitleEase.Out(u));
    }

    private Pose TitlePose(float t)
    {
        float loop = holdDuration + zoomInDuration + legDuration * 3f + cutDuration;
        float margin = size - size / zoom;
        t %= loop;

        if (t < holdDuration)
            return new Pose(1f, 0f, 0f, 0f, false);
        t -= holdDuration;

        if (t < zoomInDuration)
        {
            float u = TitleEase.InOut(t / zoomInDuration);
            return new Pose(Mathf.Lerp(1f, zoom, u), 0f, 0f, zoomedShade * u, true);
        }
        t -= zoomInDuration;

        if (t < legDuration * 3f)
        {
            int leg = Mathf.Min((int)(t / legDuration), 2);
            float u = TitleEase.InOut((t - leg * legDuration) / legDuration);
            Vector2 point = Vector2.Lerp(PanPath[leg], PanPath[leg + 1], u) * margin;
            return new Pose(zoom, point.x, point.y, zoomedShade, false);
        }
        t -= legDuration * 3f;

        return t < cutDuration / 2f
            ? new Pose(zoom, margin, margin, zoomedShade, true)
            : new Pose(1f, 0f, 0f, 0f, true);
    }

    private Pose DriftPose(float t)
    {
        float scale = 1.1f + 0.04f * Mathf.Sin(t * 2f * Mathf.PI / 14f);
        float margin = size - size / scale;
        float x = margin * (0.5f + 0.45f * Mathf.Sin(t * 2f * Mathf.PI / 23f));
        float y = margin * (0.5f + 0.45f * Mathf.Cos(t * 2f * Mathf.PI / 19f));
        return new Pose(scale, x, y, 0f, false);
    }

    private void Scramble()
    {
        jitter = Random.Range(-18f, 18f);
        leftTint.anchoredPosition = new Vector2(Random.Range(-22f, -6f), 0f);
        rightTint.anchoredPosition = new Vector2(Random.Range(6f, 22f), 0f);

        foreach (var slice in slices)
        {
            float top = Random.Range(0f, 0.9f) * size;
            float height = Random.Range(0.03f, 0.1f) * size;
            slice.anchoredPosition = new Vector2(Random.Range(-60f, 60f), -top);
            slice.sizeDelta = new Vector2(slice.sizeDelta.x, height);
            ((RectTransform)slice.GetChild(0)).anchoredPosition = new Vector2(0f, top);
        }
    }

    private void SetGlitchVisible(bool isVisible)
    {
        if (isGlitchVisible == isVisible)
            return;

        isGlitchVisible = isVisible;
        leftTint.gameObject.SetActive(isVisible);
        rightTint.gameObject.SetActive(isVisible);
        foreach (var slice in slices)
            slice.gameObject.SetActive(isVisible);
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
