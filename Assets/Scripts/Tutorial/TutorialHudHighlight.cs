using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHudHighlight : MonoBehaviour
{
    [Tooltip("指し示す HUD がある Canvas（GameScene のルートの GameObject）の名前")]
    [SerializeField] private string hudCanvasName = "UICanvas";

    [Tooltip("対象を囲む枠 対象とその子の表示中の画像・文字を合わせた範囲に、毎フレーム位置と大きさを合わせる")]
    [SerializeField] private RectTransform frame;

    [Tooltip("枠の外を暗くする四角 枠の上・下・左・右の順。枠と同じ親に置く")]
    [SerializeField] private RectTransform[] shades = new RectTransform[4];

    [Tooltip("対象の外側に枠を広げる幅")]
    [Min(0f)]
    [SerializeField] private float padding = 8f;

    private readonly List<Transform> targets = new List<Transform>();
    private readonly List<Graphic> graphics = new List<Graphic>();
    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform area;
    private Vector2 min;
    private Vector2 max;

    private void Awake()
    {
        area = (RectTransform)frame.parent;
        SetVisible(false);
    }

    public void Show(string[] paths)
    {
        targets.Clear();
        if (paths != null && paths.Length > 0)
            CollectTargets(paths);

        Refresh();
    }

    private void CollectTargets(string[] paths)
    {
        GameObject hudCanvas = GameObject.Find("/" + hudCanvasName);
        if (hudCanvas == null)
        {
            Debug.LogWarning($"{hudCanvasName} が見つからないため、HUD を指し示せません", this);
            return;
        }

        foreach (string path in paths)
        {
            Transform target = hudCanvas.transform.Find(path);
            if (target != null)
                targets.Add(target);
            else
                Debug.LogWarning($"{hudCanvasName}/{path} が見つからないため、指し示せません", this);
        }
    }

    private void LateUpdate()
    {
        Refresh();
    }

    private void Refresh()
    {
        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);
        bool found = false;

        foreach (Transform target in targets)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
                continue;

            target.GetComponentsInChildren(false, graphics);
            foreach (Graphic graphic in graphics)
            {
                if (graphic.enabled && graphic.color.a > 0f && TryGetCorners(graphic))
                {
                    Encapsulate();
                    found = true;
                }
            }
        }

        if (frame.gameObject.activeSelf != found)
            SetVisible(found);
        if (!found)
            return;

        Vector2 frameMin = min - Vector2.one * padding;
        Vector2 frameMax = max + Vector2.one * padding;
        Place(frame, frameMin, frameMax);

        Rect whole = area.rect;
        Place(shades[0], new Vector2(whole.xMin, frameMax.y), new Vector2(whole.xMax, whole.yMax));
        Place(shades[1], new Vector2(whole.xMin, whole.yMin), new Vector2(whole.xMax, frameMin.y));
        Place(shades[2], new Vector2(whole.xMin, frameMin.y), new Vector2(frameMin.x, frameMax.y));
        Place(shades[3], new Vector2(frameMax.x, frameMin.y), new Vector2(whole.xMax, frameMax.y));
    }

    private static void Place(RectTransform rect, Vector2 lower, Vector2 upper)
    {
        Vector2 size = Vector2.Max(upper - lower, Vector2.zero);
        rect.localPosition = lower + size * 0.5f;
        rect.sizeDelta = size;
    }

    private void SetVisible(bool visible)
    {
        frame.gameObject.SetActive(visible);
        foreach (RectTransform shade in shades)
            shade.gameObject.SetActive(visible);
    }

    private bool TryGetCorners(Graphic graphic)
    {
        if (!(graphic is TMP_Text text))
        {
            graphic.rectTransform.GetWorldCorners(corners);
            return true;
        }

        Bounds bounds = text.textBounds;
        if (!(bounds.size.x > 0f && bounds.size.y > 0f))
            return false;

        Vector3 lower = bounds.min;
        Vector3 upper = bounds.max;
        Transform textTransform = text.transform;
        corners[0] = textTransform.TransformPoint(new Vector3(lower.x, lower.y, 0f));
        corners[1] = textTransform.TransformPoint(new Vector3(lower.x, upper.y, 0f));
        corners[2] = textTransform.TransformPoint(new Vector3(upper.x, upper.y, 0f));
        corners[3] = textTransform.TransformPoint(new Vector3(upper.x, lower.y, 0f));
        return true;
    }

    private void Encapsulate()
    {
        foreach (Vector3 corner in corners)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, corner, null, out Vector2 local);
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }
    }
}
