using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TriangleGraphic : MaskableGraphic
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    [Tooltip("三角の頂点が向く方向")]
    [SerializeField] private Direction direction = Direction.Right;

    public void SetDirection(Direction value)
    {
        direction = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Color32 vertexColor = color;
        switch (direction)
        {
            case Direction.Left:
                AddTriangle(vh, vertexColor, new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.center.y));
                break;
            case Direction.Right:
                AddTriangle(vh, vertexColor, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.center.y));
                break;
            case Direction.Up:
                AddTriangle(vh, vertexColor, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.center.x, rect.yMax), new Vector2(rect.xMax, rect.yMin));
                break;
            case Direction.Down:
                AddTriangle(vh, vertexColor, new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), new Vector2(rect.center.x, rect.yMin));
                break;
        }
    }

    private static void AddTriangle(VertexHelper vh, Color32 vertexColor, Vector2 a, Vector2 b, Vector2 c)
    {
        vh.AddVert(a, vertexColor, Vector2.zero);
        vh.AddVert(b, vertexColor, Vector2.zero);
        vh.AddVert(c, vertexColor, Vector2.zero);
        vh.AddTriangle(0, 1, 2);
    }
}
