using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// オーバーレイ描画結果を Unity UI へ反映するビュー。
/// </summary>
public class PlayerReactionOverlayView
{
    private readonly Image blackOverlayImage;
    private readonly ScreenVignetteGraphic edgeOverlayGraphic;

    public PlayerReactionOverlayView(
        Image blackOverlayImage,
        ScreenVignetteGraphic edgeOverlayGraphic,
        float vignetteInnerRadius,
        float vignetteOuterRadius)
    {
        this.blackOverlayImage = blackOverlayImage;
        this.edgeOverlayGraphic = edgeOverlayGraphic;

        if (this.blackOverlayImage != null)
        {
            this.blackOverlayImage.raycastTarget = false;
        }

        if (this.edgeOverlayGraphic != null)
        {
            this.edgeOverlayGraphic.raycastTarget = false;
            this.edgeOverlayGraphic.InnerRadius = vignetteInnerRadius;
            this.edgeOverlayGraphic.OuterRadius = vignetteOuterRadius;
        }
    }

    public void Render(OverlayEffectFrame frame)
    {
        if (blackOverlayImage != null)
        {
            blackOverlayImage.color = new Color(0f, 0f, 0f, frame.BlackAlpha);
        }

        if (edgeOverlayGraphic != null)
        {
            Color color = frame.EdgeColor;
            color.a = frame.EdgeAlpha;
            edgeOverlayGraphic.color = color;
        }
    }

    public void Reset()
    {
        Render(default);
    }
}
