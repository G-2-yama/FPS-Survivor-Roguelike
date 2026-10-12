using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SlantedGraphic : MaskableGraphic
{
    [Tooltip("右上の角を左へ削る幅（px）")]
    [SerializeField] private float topRightCut;

    [Tooltip("右下の角を左へ削る幅（px）")]
    [SerializeField] private float bottomRightCut;

    public void SetCuts(float topRight, float bottomRight)
    {
        topRightCut = topRight;
        bottomRightCut = bottomRight;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Color32 vertexColor = color;
        vh.AddVert(new Vector3(rect.xMin, rect.yMin), vertexColor, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin, rect.yMax), vertexColor, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax - topRightCut, rect.yMax), vertexColor, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax - bottomRightCut, rect.yMin), vertexColor, Vector2.zero);
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
