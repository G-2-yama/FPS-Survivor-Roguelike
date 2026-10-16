using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotTooltipView : MonoBehaviour
{
    [Tooltip("位置と大きさを変える枠")]
    [SerializeField] private RectTransform panel;
    [Tooltip("装備名")]
    [SerializeField] private TMP_Text titleText;
    [Tooltip("説明文")]
    [SerializeField] private TMP_Text descriptionText;
    [Tooltip("能力値の一覧")]
    [SerializeField] private TMP_Text statsText;
    [Tooltip("スロットとの間隔")]
    [SerializeField] private float gap = 12f;

    private static readonly Vector3[] AnchorCorners = new Vector3[4];
    private static readonly Vector3[] BoundsCorners = new Vector3[4];

    public void Show(SlotTooltipContent content, RectTransform anchor)
    {
        titleText.text = content.Title;
        SetText(descriptionText, content.Description);
        SetText(statsText, content.Stats);

        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        PlaceBeside(anchor);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static void SetText(TMP_Text text, string value)
    {
        bool hasValue = !string.IsNullOrEmpty(value);
        text.gameObject.SetActive(hasValue);
        text.text = hasValue ? value : "";
    }

    private void PlaceBeside(RectTransform anchor)
    {
        var bounds = (RectTransform)panel.GetComponentInParent<Canvas>().rootCanvas.transform;
        anchor.GetWorldCorners(AnchorCorners);
        bounds.GetWorldCorners(BoundsCorners);

        Vector3 scale = panel.lossyScale;
        float width = panel.rect.width * scale.x;
        float height = panel.rect.height * scale.y;
        float worldGap = gap * scale.x;

        bool fitsRight = AnchorCorners[2].x + worldGap + width <= BoundsCorners[2].x;
        float x = fitsRight ? AnchorCorners[2].x + worldGap : AnchorCorners[1].x - worldGap - width;
        float top = Mathf.Max(AnchorCorners[1].y, BoundsCorners[0].y + height);
        top = Mathf.Min(top, BoundsCorners[1].y);

        panel.pivot = new Vector2(0f, 1f);
        panel.position = new Vector3(x, top, panel.position.z);
    }
}
