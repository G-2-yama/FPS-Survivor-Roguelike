using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SettingBarGraphic : MaskableGraphic
{
    [Tooltip("目盛りの数")]
    [SerializeField, Min(1)] private int steps = GameSettings.MaxLevel;

    [Tooltip("目盛りの間隔（px）")]
    [SerializeField] private float gap = 6f;

    [Tooltip("目盛りを右へ傾ける角度（度）")]
    [SerializeField] private float skewAngle = 20f;

    [Tooltip("値に届いていない目盛りの色")]
    [SerializeField] private Color offColor = new Color(1f, 1f, 1f, 0.1f);

    [Tooltip("値に届いた目盛りの色")]
    [SerializeField] private Color onColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("行を選んでいるときの、値に届いた目盛りの色")]
    [SerializeField] private Color highlightColor = new Color(0.56f, 0.86f, 1f);

    private int level;
    private bool highlighted;

    public int Steps => steps;

    public int Level
    {
        get => level;
        set
        {
            if (level == value)
                return;
            level = value;
            SetVerticesDirty();
        }
    }

    public bool Highlighted
    {
        get => highlighted;
        set
        {
            if (highlighted == value)
                return;
            highlighted = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float width = (rect.width - gap * (steps - 1)) / steps;
        float shift = Mathf.Tan(skewAngle * Mathf.Deg2Rad) * rect.height * 0.5f;
        Color on = highlighted ? highlightColor : onColor;

        for (int i = 0; i < steps; i++)
        {
            Color32 vertexColor = (i < level ? on : offColor) * color;
            float x = rect.xMin + i * (width + gap);
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(x - shift, rect.yMin), vertexColor, Vector2.zero);
            vh.AddVert(new Vector3(x + shift, rect.yMax), vertexColor, Vector2.zero);
            vh.AddVert(new Vector3(x + width + shift, rect.yMax), vertexColor, Vector2.zero);
            vh.AddVert(new Vector3(x + width - shift, rect.yMin), vertexColor, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
