using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class MinimapIconMesh : MaskableGraphic
{
    private const int MaxIcons = 16250;

    private readonly List<Vector4> icons = new();

    private Sprite sprite;
    private Vector2 cornerMin;
    private Vector2 cornerMax;

    public override Texture mainTexture =>
        sprite != null ? sprite.texture : s_WhiteTexture;


    public void Setup(Image source, RectTransform sourceRect)
    {
        sprite = source.sprite;
        color = source.color;
        material = source.material;
        maskable = source.maskable;
        raycastTarget = false;

        Vector2 size = Vector2.Scale(
            sourceRect.rect.size,
            sourceRect.localScale
        );

        cornerMin = -Vector2.Scale(sourceRect.pivot, size);
        cornerMax = cornerMin + size;
    }


    public void ClearIcons()
    {
        icons.Clear();
    }


    public void AddIcon(Vector2 position, float cos, float sin)
    {
        if (icons.Count >= MaxIcons)
        {
            return;
        }

        icons.Add(new Vector4(position.x, position.y, cos, sin));
    }


    public void ApplyIcons()
    {
        SetVerticesDirty();
    }


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector4 uv = sprite != null
            ? DataUtility.GetOuterUV(sprite)
            : new Vector4(0f, 0f, 1f, 1f);

        Color32 color32 = color;

        for (int i = 0; i < icons.Count; i++)
        {
            Vector4 icon = icons[i];
            int index = vh.currentVertCount;

            vh.AddVert(Corner(icon, cornerMin.x, cornerMin.y), color32, new Vector4(uv.x, uv.y));
            vh.AddVert(Corner(icon, cornerMin.x, cornerMax.y), color32, new Vector4(uv.x, uv.w));
            vh.AddVert(Corner(icon, cornerMax.x, cornerMax.y), color32, new Vector4(uv.z, uv.w));
            vh.AddVert(Corner(icon, cornerMax.x, cornerMin.y), color32, new Vector4(uv.z, uv.y));

            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index + 2, index + 3, index);
        }
    }


    private static Vector3 Corner(Vector4 icon, float x, float y)
    {
        return new Vector3(
            icon.x + x * icon.z - y * icon.w,
            icon.y + x * icon.w + y * icon.z,
            0f
        );
    }
}
