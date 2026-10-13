using UnityEngine;

public class ChargeBeamWidth : MonoBehaviour
{
    [Tooltip("撃った瞬間の太さの倍率")]
    [SerializeField, Min(1f)] private float burstScale = 2.5f;

    [Tooltip("撃った瞬間の太さから通常の太さに戻るまでの秒数")]
    [SerializeField, Min(0.01f)] private float burstDuration = 0.3f;

    [Tooltip("寿命の終わりに細くなって消えるまでの秒数")]
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;

    [Tooltip("太さの揺れの大きさ 0 で揺れなし")]
    [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.15f;

    [Tooltip("太さの揺れの速さ(回/秒)")]
    [SerializeField, Min(0f)] private float pulseFrequency = 8f;

    private LineRenderer[] lines;
    private float[] baseWidths;
    private DamageBase damage;
    private float elapsed;

    private void Awake()
    {
        lines = GetComponentsInChildren<LineRenderer>(true);
        baseWidths = new float[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            baseWidths[i] = lines[i].widthMultiplier;
        }
        damage = GetComponent<DamageBase>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
        ApplyScale(burstScale);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        ApplyScale(CurrentScale());
    }

    private float CurrentScale()
    {
        float burst = Mathf.Lerp(burstScale, 1f, Mathf.SmoothStep(0f, 1f, elapsed / burstDuration));
        float pulse = 1f + pulseAmount * Mathf.Sin(elapsed * pulseFrequency * 2f * Mathf.PI);
        float fade = 1f;
        if (damage != null && damage.Lifetime > 0f)
        {
            fade = Mathf.Clamp01((damage.Lifetime - elapsed) / fadeDuration);
        }
        return burst * pulse * fade;
    }

    private void ApplyScale(float scale)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].widthMultiplier = baseWidths[i] * scale;
        }
    }
}
