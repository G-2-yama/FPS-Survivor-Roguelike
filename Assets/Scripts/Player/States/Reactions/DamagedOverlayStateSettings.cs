using UnityEngine;

/// <summary>
/// 被ダメージオーバーレイ演出の調整値を保持する。
/// </summary>
[CreateAssetMenu(menuName = "Player/Reactions/Damaged Overlay State Settings")]
public class DamagedOverlayStateSettings : ScriptableObject
{
    [Header("Intensity")]
    [SerializeField, Range(0f, 1f)] private float minIntensity = 0.35f;
    [SerializeField, Range(0f, 2f)] private float damageToIntensityScale = 4f;
    [SerializeField, Range(0f, 1f)] private float stackedIntensityGain = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxBlackAlpha = 0.18f;
    [SerializeField, Range(0f, 1f)] private float maxEdgeAlpha = 0.5f;
    [SerializeField] private Color edgeTint = new(0.85f, 0.1f, 0.08f, 1f);

    [Header("Timing")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.06f;
    [SerializeField, Min(0f)] private float holdDuration = 0.08f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.35f;

    public float MinIntensity => minIntensity;

    public float DamageToIntensityScale => damageToIntensityScale;

    public float StackedIntensityGain => stackedIntensityGain;

    public float MaxBlackAlpha => maxBlackAlpha;

    public float MaxEdgeAlpha => maxEdgeAlpha;

    public Color EdgeTint => edgeTint;

    public float FadeInDuration => fadeInDuration;

    public float HoldDuration => holdDuration;

    public float FadeOutDuration => fadeOutDuration;
}
