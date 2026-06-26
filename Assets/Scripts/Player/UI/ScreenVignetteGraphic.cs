using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面中央を抜き、外周だけに色を乗せる簡易ビネット描画。
/// </summary>
[AddComponentMenu("UI/Effects/Screen Vignette Graphic")]
public class ScreenVignetteGraphic : MaskableGraphic
{
    [SerializeField, Range(0f, 1f)] private float innerRadius = 0.55f;
    [SerializeField, Range(0f, 1.5f)] private float outerRadius = 1f;
    [SerializeField, Range(2, 32)] private int horizontalSegments = 12;
    [SerializeField, Range(2, 32)] private int verticalSegments = 12;

    public float InnerRadius
    {
        get => innerRadius;
        set
        {
            innerRadius = Mathf.Clamp01(value);
            SetVerticesDirty();
        }
    }

    public float OuterRadius
    {
        get => outerRadius;
        set
        {
            outerRadius = Mathf.Max(innerRadius + 0.001f, value);
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        int xSegments = Mathf.Max(2, horizontalSegments);
        int ySegments = Mathf.Max(2, verticalSegments);
        float maxDistance = Mathf.Sqrt(0.5f);

        for (int y = 0; y <= ySegments; y++)
        {
            float fy = (float)y / ySegments;
            float py = Mathf.Lerp(rect.yMin, rect.yMax, fy);

            for (int x = 0; x <= xSegments; x++)
            {
                float fx = (float)x / xSegments;
                float px = Mathf.Lerp(rect.xMin, rect.xMax, fx);

                float centeredX = fx - 0.5f;
                float centeredY = fy - 0.5f;
                float normalizedDistance = Mathf.Sqrt(centeredX * centeredX + centeredY * centeredY) / maxDistance;
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(innerRadius, outerRadius, normalizedDistance));

                UIVertex vertex = UIVertex.simpleVert;
                vertex.position = new Vector3(px, py);
                vertex.uv0 = new Vector2(fx, fy);
                vertex.color = new Color(color.r, color.g, color.b, color.a * alpha);
                vh.AddVert(vertex);
            }
        }

        int rowWidth = xSegments + 1;
        for (int y = 0; y < ySegments; y++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                int bottomLeft = y * rowWidth + x;
                int topLeft = bottomLeft + rowWidth;
                int topRight = topLeft + 1;
                int bottomRight = bottomLeft + 1;

                vh.AddTriangle(bottomLeft, topLeft, topRight);
                vh.AddTriangle(bottomLeft, topRight, bottomRight);
            }
        }
    }
}
