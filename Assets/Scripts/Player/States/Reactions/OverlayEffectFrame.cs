using UnityEngine;

/// <summary>
/// 1フレーム分のオーバーレイ描画結果。
/// </summary>
public struct OverlayEffectFrame
{
    public float BlackAlpha;
    public Color EdgeColor;
    public float EdgeAlpha;

    public void AddBlack(float alpha)
    {
        BlackAlpha = Mathf.Max(BlackAlpha, alpha);
    }

    public void AddEdge(Color color, float alpha)
    {
        if (alpha <= 0f)
        {
            return;
        }

        EdgeColor += color * alpha;
        EdgeAlpha += alpha;
    }

    public void Normalize()
    {
        BlackAlpha = Mathf.Clamp01(BlackAlpha);

        if (EdgeAlpha > 0f)
        {
            EdgeColor /= EdgeAlpha;
        }

        EdgeAlpha = Mathf.Clamp01(EdgeAlpha);
    }
}
