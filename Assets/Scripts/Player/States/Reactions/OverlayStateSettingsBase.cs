using UnityEngine;

/// <summary>
/// オーバーレイ演出設定の共通ベース。
/// </summary>
public abstract class OverlayStateSettingsBase : ScriptableObject
{
    [Header("Intensity")]
    [SerializeField, Range(0f, 1f)] private float minimumIntensity = 0.35f;
    [SerializeField, Range(0f, 2f)] private float responseScale = 4f;
    [SerializeField, Range(0f, 1f)] private float intensityCarryover = 0.25f;

    [Header("Visual")]
    [SerializeField] private Color overlayColor = new(0.85f, 0.1f, 0.08f, 1f);
    [SerializeField, Range(0f, 1f)] private float screenDarknessAtFullIntensity = 0.18f;
    [SerializeField, Range(0f, 1f)] private float edgeOpacityAtFullIntensity = 0.5f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.06f;
    [SerializeField, Min(0f)] private float holdDuration = 0.08f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.35f;

    public float MinimumIntensity => minimumIntensity;

    public float ResponseScale => responseScale;

    public float IntensityCarryover => intensityCarryover;

    public Color OverlayColor => overlayColor;

    public float ScreenDarknessAtFullIntensity => screenDarknessAtFullIntensity;

    public float EdgeOpacityAtFullIntensity => edgeOpacityAtFullIntensity;

    public float FadeInDuration => fadeInDuration;

    public float HoldDuration => holdDuration;

    public float FadeOutDuration => fadeOutDuration;
}
