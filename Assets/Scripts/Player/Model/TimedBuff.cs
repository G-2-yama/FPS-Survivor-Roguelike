using System;

public class TimedBuff
{
    private float remainingTime;

    private readonly Action applyAction;
    private readonly Action removeAction;

    public bool IsFinished => remainingTime <= 0f;
    public float RemainingTime => Math.Max(0f, remainingTime);
    public virtual PickupEffectDefinition Definition => null;
    protected void ResetDuration(float duration) => remainingTime = duration;

    /// <summary>同じ効果として取り込める場合、派生クラスで期限を延長する</summary>
    public virtual bool TryMerge(TimedBuff other) => false;

    /// <summary>適用済みの効果を維持し、残り時間に加算する</summary>
    protected void ExtendDuration(float duration)
    {
        remainingTime = Math.Max(0f, remainingTime) + duration;
    }

    public TimedBuff(
        float duration,
        Action applyAction,
        Action removeAction)
    {
        remainingTime = duration;

        this.applyAction = applyAction;
        this.removeAction = removeAction;
    }

    public virtual void Apply()
    {
        applyAction?.Invoke();
    }

    public void Update(float deltaTime)
    {
        remainingTime -= deltaTime;
    }

    public virtual void Remove()
    {
        removeAction?.Invoke();
    }
}