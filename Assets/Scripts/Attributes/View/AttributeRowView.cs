using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributeRowView : MonoBehaviour
{
    [Tooltip("行の先頭に置くバッジのプレハブ")]
    [SerializeField] private AttributeBadgeView badgePrefab;
    [Tooltip("ゲージのマスを並べる親")]
    [SerializeField] private RectTransform gauge;
    [Tooltip("ゲージをまとめて薄くする")]
    [SerializeField] private CanvasGroup gaugeGroup;
    [Tooltip("マスの見本。複製して使う")]
    [SerializeField] private Image pipTemplate;
    [Tooltip("基盤で有効化の区切りの後に入れる隙間")]
    [SerializeField] private RectTransform thresholdGap;
    [Tooltip("モジュールで有効化の数を超えた分（+1 など）")]
    [SerializeField] private TMP_Text overText;
    [Tooltip("数が変わらないときに出す注記")]
    [SerializeField] private TMP_Text unchangedText;
    [Tooltip("属性の色")]
    [SerializeField] private UIPalette palette;

    [Header("Pips")]
    [Tooltip("埋まったマスの絵。空なら四角")]
    [SerializeField] private Sprite filledPipSprite;
    [Tooltip("空いたマスの絵")]
    [SerializeField] private Sprite emptyPipSprite;
    [Tooltip("有効化に届いていないときの、取る前からあるマスの色")]
    [SerializeField] private Color filledPipColor = Color.white;

    [Header("Unchanged")]
    [Tooltip("数が変わらないときのゲージと注記の透明度")]
    [SerializeField, Range(0f, 1f)] private float unchangedAlpha = 0.55f;

    [Header("Blink")]
    [Tooltip("この候補で増えるマスの点滅の周期（秒）")]
    [SerializeField, Min(0.05f)] private float blinkPeriod = 0.9f;
    [Tooltip("点滅で暗くなったときの透明度")]
    [SerializeField, Range(0f, 1f)] private float blinkAlpha = 0.35f;

    private AttributeBadgeView badge;
    private readonly List<Image> pips = new List<Image>();
    private readonly List<Image> blinkingPips = new List<Image>();

    public void Setup(AttributeDefinition attribute, int before, int after)
    {
        if (badge == null)
        {
            badge = Instantiate(badgePrefab, transform);
            badge.transform.SetAsFirstSibling();
        }

        AttributeDisplayState state = AttributeDisplayStates.Of(attribute, after);
        badge.Setup(attribute, state);

        bool isBase = attribute.Kind == AttributeKind.Base;
        int total = isBase ? attribute.MaxThreshold : attribute.ActivateThreshold;
        SetupPips(attribute, state, total, Mathf.Min(before, after), after);

        int over = after - total;
        overText.gameObject.SetActive(over > 0);
        if (over > 0)
        {
            overText.text = $"+{over}";
            overText.color = state.ToColor(palette, filledPipColor);
        }

        bool unchanged = before == after;
        gaugeGroup.alpha = unchanged ? unchangedAlpha : 1f;
        unchangedText.gameObject.SetActive(unchanged);
        unchangedText.alpha = unchangedAlpha;
    }

    private void SetupPips(AttributeDefinition attribute, AttributeDisplayState state, int total, int kept, int after)
    {
        while (pips.Count < total)
            pips.Add(Instantiate(pipTemplate, gauge));

        bool showGap = attribute.Kind == AttributeKind.Base && attribute.ActivateThreshold < total;
        thresholdGap.gameObject.SetActive(showGap);

        Color filledColor = state.ToColor(palette, filledPipColor);
        Color newColor = state == AttributeDisplayState.Max ? palette.AttributeMaxColor : palette.AttributeActiveColor;

        blinkingPips.Clear();
        int sibling = 0;
        for (int i = 0; i < pips.Count; i++)
        {
            Image pip = pips[i];
            bool used = i < total;
            pip.gameObject.SetActive(used);
            pip.canvasRenderer.SetAlpha(1f);
            if (!used)
                continue;

            if (showGap && i == attribute.ActivateThreshold)
                thresholdGap.SetSiblingIndex(sibling++);
            pip.transform.SetSiblingIndex(sibling++);

            if (i < kept)
            {
                pip.sprite = filledPipSprite;
                pip.color = filledColor;
            }
            else if (i < after)
            {
                pip.sprite = filledPipSprite;
                pip.color = newColor;
                if (state != AttributeDisplayState.Max)
                    blinkingPips.Add(pip);
            }
            else
            {
                pip.sprite = emptyPipSprite;
                pip.color = palette.AttributeIdleFrameColor;
            }
        }

        overText.transform.SetSiblingIndex(sibling);
    }

    private void Update()
    {
        if (blinkingPips.Count == 0)
            return;

        float alpha = Mathf.Repeat(Time.unscaledTime, blinkPeriod) < blinkPeriod * 0.5f ? 1f : blinkAlpha;
        foreach (Image pip in blinkingPips)
            pip.canvasRenderer.SetAlpha(alpha);
    }
}
