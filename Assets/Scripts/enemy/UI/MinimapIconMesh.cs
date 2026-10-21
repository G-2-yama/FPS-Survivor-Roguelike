using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class MinimapIconMesh : MaskableGraphic
{
    public const int MaxIcons = 16250;

    private Sprite sprite;
    private Vector4 outerUV = new Vector4(0f, 0f, 1f, 1f);

    private Mesh mesh;
    private NativeArray<Vector3> vertices;
    private NativeArray<Color32> colors;
    private NativeArray<Vector2> uvs;
    private int[] triangles = System.Array.Empty<int>();
    private int iconCount;

    public Vector2 CornerMin { get; private set; }
    public Vector2 CornerMax { get; private set; }

    public override Texture mainTexture =>
        sprite != null ? sprite.texture : s_WhiteTexture;


    public void Setup(Image source, RectTransform sourceRect)
    {
        sprite = source.sprite;
        color = source.color;
        material = source.material;
        maskable = source.maskable;
        raycastTarget = false;

        if (sprite != null)
        {
            outerUV = DataUtility.GetOuterUV(sprite);
        }

        Vector2 size = Vector2.Scale(
            sourceRect.rect.size,
            sourceRect.localScale
        );

        CornerMin = -Vector2.Scale(sourceRect.pivot, size);
        CornerMax = CornerMin + size;
    }


    public NativeArray<Vector3> PrepareVertices(int count, IReadOnlyList<Color32> iconColors = null)
    {
        iconCount = Mathf.Min(count, MaxIcons);
        EnsureCapacity(iconCount);
        for (int i = 0; i < iconCount; i++)
        {
            Color32 iconColor = iconColors != null ? iconColors[i] : (Color32)color;
            int v = i * 4;
            colors[v] = iconColor;
            colors[v + 1] = iconColor;
            colors[v + 2] = iconColor;
            colors[v + 3] = iconColor;
        }
        return vertices;
    }


    public void ApplyIcons()
    {
        SetVerticesDirty();
    }


    protected override void UpdateGeometry()
    {
        if (mesh == null)
        {
            mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.MarkDynamic();
        }

        mesh.Clear();

        if (iconCount > 0)
        {
            int vertexCount = iconCount * 4;

            mesh.SetVertices(vertices, 0, vertexCount);
            mesh.SetColors(colors, 0, vertexCount);
            mesh.SetUVs(0, uvs, 0, vertexCount);
            mesh.SetTriangles(triangles, 0, iconCount * 6, 0);
        }

        canvasRenderer.SetMesh(mesh);
    }


    protected override void OnDestroy()
    {
        DisposeBuffers();

        if (mesh != null)
        {
            Destroy(mesh);
        }

        base.OnDestroy();
    }


    private void EnsureCapacity(int count)
    {
        int currentCapacity = vertices.IsCreated ? vertices.Length / 4 : 0;

        if (currentCapacity >= count)
        {
            return;
        }

        int capacity = Mathf.Min(
            Mathf.Max(count, Mathf.Max(currentCapacity * 2, 64)),
            MaxIcons
        );

        DisposeBuffers();

        vertices = new NativeArray<Vector3>(capacity * 4, Allocator.Persistent);
        colors = new NativeArray<Color32>(capacity * 4, Allocator.Persistent);
        uvs = new NativeArray<Vector2>(capacity * 4, Allocator.Persistent);
        triangles = new int[capacity * 6];

        Color32 color32 = color;

        for (int i = 0; i < capacity; i++)
        {
            int v = i * 4;

            colors[v] = color32;
            colors[v + 1] = color32;
            colors[v + 2] = color32;
            colors[v + 3] = color32;

            uvs[v] = new Vector2(outerUV.x, outerUV.y);
            uvs[v + 1] = new Vector2(outerUV.x, outerUV.w);
            uvs[v + 2] = new Vector2(outerUV.z, outerUV.w);
            uvs[v + 3] = new Vector2(outerUV.z, outerUV.y);

            int t = i * 6;

            triangles[t] = v;
            triangles[t + 1] = v + 1;
            triangles[t + 2] = v + 2;
            triangles[t + 3] = v + 2;
            triangles[t + 4] = v + 3;
            triangles[t + 5] = v;
        }
    }


    private void DisposeBuffers()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (colors.IsCreated) colors.Dispose();
        if (uvs.IsCreated) uvs.Dispose();
    }
}
