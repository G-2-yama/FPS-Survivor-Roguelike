using UnityEngine;

/// <summary>
/// 回復時の画面オーバーレイ演出を管理する。
/// </summary>
public class HealedOverlayState : IOverlayState
{
    private enum Phase
    {
        Idle,
        FadeIn,
        Hold,
        FadeOut,
    }

    private readonly HealedOverlayStateSettings settings;

    private float currentIntensity;
    private float startIntensity;
    private float targetIntensity;
    private float timer;
    private Phase phase = Phase.Idle;

    public PlayerOverlayTrigger Trigger => PlayerOverlayTrigger.Healed;

    public HealedOverlayState(HealedOverlayStateSettings settings)
    {
        this.settings = settings;
    }

    public void TriggerEffect(int amount, int maxAmount)
    {
        float normalizedValue = maxAmount > 0 ? (float)amount / maxAmount : 0f;
        float nextIntensity = Mathf.Clamp01(
            Mathf.Max(MinimumIntensity, normalizedValue * ResponseScale)
            + currentIntensity * IntensityCarryover);

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
        frame.AddBlack(ScreenDarknessAtFullIntensity * currentIntensity);
        frame.AddEdge(OverlayColor, EdgeOpacityAtFullIntensity * currentIntensity);
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

    private float MinimumIntensity => settings != null ? settings.MinimumIntensity : 0.2f;

    private float ResponseScale => settings != null ? settings.ResponseScale : 2f;

    private float IntensityCarryover => settings != null ? settings.IntensityCarryover : 0.15f;

    private float FadeInDuration => settings != null ? settings.FadeInDuration : 0.05f;

    private float HoldDuration => settings != null ? settings.HoldDuration : 0.06f;

    private float FadeOutDuration => settings != null ? settings.FadeOutDuration : 0.28f;

    private float ScreenDarknessAtFullIntensity => settings != null ? settings.ScreenDarknessAtFullIntensity : 0f;

    private float EdgeOpacityAtFullIntensity => settings != null ? settings.EdgeOpacityAtFullIntensity : 0.3f;

    private Color OverlayColor => settings != null ? settings.OverlayColor : new Color(0.2f, 0.95f, 0.45f, 1f);
}
