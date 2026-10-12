using UnityEngine;
using UnityEngine.UI;

public class TitleLogoGlitch : MonoBehaviour
{
    [Tooltip("左にずらす赤のシルエット")]
    [SerializeField] private Graphic red;

    [Tooltip("右にずらすシアンのシルエット")]
    [SerializeField] private Graphic cyan;

    [Tooltip("一周の時間（秒） 終わり際に一瞬だけ大きくずれます")]
    [SerializeField] private float period = 2.6f;

    private RectTransform rect;
    private Vector2 home;

    private void Awake()
    {
        rect = (RectTransform)transform;
        home = rect.anchoredPosition;
    }

    private void Update()
    {
        float phase = Time.unscaledTime % period / period;

        if (phase >= 0.88f && phase < 0.9f)
            Apply(6f, -14f, 12f, 0.9f);
        else if (phase >= 0.9f && phase < 0.92f)
            Apply(-8f, 10f, -10f, 0.9f);
        else
            Apply(0f, -3f, 3f, 0.7f);
    }

    private void Apply(float shift, float redShift, float cyanShift, float alpha)
    {
        rect.anchoredPosition = home + new Vector2(shift, 0f);
        Place(red, redShift, alpha);
        Place(cyan, cyanShift, alpha);
    }

    private static void Place(Graphic graphic, float shift, float alpha)
    {
        graphic.rectTransform.anchoredPosition = new Vector2(shift, 0f);
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
