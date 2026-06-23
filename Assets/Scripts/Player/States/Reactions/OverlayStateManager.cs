/// <summary>
/// オーバーレイ状態の発火先と毎フレーム更新を束ねる。
/// </summary>
public class OverlayStateManager
{
    private readonly IOverlayState[] states;

    public OverlayStateManager(IOverlayState[] states)
    {
        this.states = states ?? System.Array.Empty<IOverlayState>();
    }

    public void Trigger(PlayerOverlayTrigger trigger, int amount, int maxAmount)
    {
        foreach (IOverlayState state in states)
        {
            if (state != null && state.Trigger == trigger)
            {
                state.TriggerEffect(amount, maxAmount);
            }
        }
    }

    public OverlayEffectFrame Tick(float deltaTime)
    {
        OverlayEffectFrame frame = default;

        foreach (IOverlayState state in states)
        {
            if (state == null)
            {
                continue;
            }

            state.Tick(deltaTime);
            state.Accumulate(ref frame);
        }

        frame.Normalize();
        return frame;
    }

    public void Reset()
    {
        foreach (IOverlayState state in states)
        {
            state?.Reset();
        }
    }
}
