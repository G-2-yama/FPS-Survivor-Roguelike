using UnityEngine;
using UnityEngine.UI;

/// <summary>1つのバフの画像と終了前の点滅だけを担当する。</summary>
[RequireComponent(typeof(Image))]
public sealed class BuffIconView : MonoBehaviour
{
    private Image image;
    private float blinkTime;
    public void Initialize(Sprite sprite)
    {
        image = GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    public void Render(float remainingTime, float deltaTime, float blinkBeforeSeconds, float blinkPeriod)
    {
        bool blinking = remainingTime > 0f && remainingTime <= blinkBeforeSeconds;
        blinkTime = blinking ? blinkTime + deltaTime : 0f;
        float alpha = blinking && Mathf.Repeat(blinkTime, blinkPeriod) >= blinkPeriod * 0.5f ? 0.25f : 1f;
        image.color = new Color(1f, 1f, 1f, alpha);
    }
}