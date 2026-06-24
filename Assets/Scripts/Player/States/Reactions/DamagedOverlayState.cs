using UnityEngine;

/// <summary>
/// 被ダメージ時の画面暗転と赤ビネット演出を管理する。
/// 数値設定はこの状態自身が持つ。
/// </summary>
public class DamagedOverlayState : IOverlayState
{
    private enum Phase
    {
        Idle,
        FadeIn,
        Hold,
        FadeOut,
    }

    private readonly DamagedOverlayStateSettings settings;

    private float currentIntensity;
    private float startIntensity;
    private float targetIntensity;
    private float timer;
    private Phase phase = Phase.Idle;

    public PlayerOverlayTrigger Trigger => PlayerOverlayTrigger.Damaged;

    public DamagedOverlayState(DamagedOverlayStateSettings settings)
    {
        this.settings = settings;
    }

    public void TriggerEffect(int amount, int maxAmount)
    {
        float normalizedValue = maxAmount > 0 ? (float)amount / maxAmount : 0f;
        float nextIntensity = Mathf.Clamp01(
            Mathf.Max(MinIntensity, normalizedValue * DamageToIntensityScale)
            + currentIntensity * StackedIntensityGain);

        startIntensity = currentIntensity;
        targetIntensity = nextIntensity;
        timer = 0f;
        phase = FadeInDuration <= 0f ? Phase.Hold : Phase.FadeIn;

        if (phase == Phase.Hold)
        {
            currentIntensity = targetIntensity;
        }
    }

    public void Tick(float deltaTime)
    {
        switch (phase)
        {
            case Phase.Idle:
                currentIntensity = 0f;
                break;

            case Phase.FadeIn:
                timer += deltaTime;
                currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, timer / FadeInDuration);
                if (timer >= FadeInDuration)
                {
                    currentIntensity = targetIntensity;
                    timer = 0f;
                    phase = Phase.Hold;
                }
                break;

            case Phase.Hold:
                currentIntensity = targetIntensity;
                if (HoldDuration <= 0f)
                {
                    BeginFadeOut();
                    break;
                }

                timer += deltaTime;
                if (timer >= HoldDuration)
                {
                    BeginFadeOut();
                }
                break;

            case Phase.FadeOut:
                timer += deltaTime;
                currentIntensity = Mathf.Lerp(startIntensity, 0f, timer / FadeOutDuration);
                if (timer >= FadeOutDuration)
                {
                    Reset();
                }
                break;
        }
    }

    public void Accumulate(ref OverlayEffectFrame frame)
    {
        frame.AddBlack(MaxBlackAlpha * currentIntensity);
        frame.AddEdge(EdgeTint, MaxEdgeAlpha * currentIntensity);
    }

    public void Reset()
    {
        currentIntensity = 0f;
        startIntensity = 0f;
        targetIntensity = 0f;
        timer = 0f;
        phase = Phase.Idle;
    }

    private void BeginFadeOut()
    {
        startIntensity = currentIntensity;
        timer = 0f;
        phase = FadeOutDuration <= 0f ? Phase.Idle : Phase.FadeOut;

        if (phase == Phase.Idle)
        {
            currentIntensity = 0f;
        }
    }

    private float MinIntensity => settings != null ? settings.MinIntensity : 0.35f;

    private float DamageToIntensityScale => settings != null ? settings.DamageToIntensityScale : 4f;

    private float StackedIntensityGain => settings != null ? settings.StackedIntensityGain : 0.25f;

    private float FadeInDuration => settings != null ? settings.FadeInDuration : 0.06f;

    private float HoldDuration => settings != null ? settings.HoldDuration : 0.08f;

    private float FadeOutDuration => settings != null ? settings.FadeOutDuration : 0.35f;

    private float MaxBlackAlpha => settings != null ? settings.MaxBlackAlpha : 0.18f;

    private float MaxEdgeAlpha => settings != null ? settings.MaxEdgeAlpha : 0.5f;

    private Color EdgeTint => settings != null ? settings.EdgeTint : new Color(0.85f, 0.1f, 0.08f, 1f);
}
