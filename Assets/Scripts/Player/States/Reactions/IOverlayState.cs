public interface IOverlayState
{
    PlayerOverlayTrigger Trigger { get; }

    void TriggerEffect(int amount, int maxAmount);

    void Tick(float deltaTime);

    void Accumulate(ref OverlayEffectFrame frame);

    void Reset();
}
