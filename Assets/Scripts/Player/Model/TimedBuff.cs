using System;

public class TimedBuff
{
    private float remainingTime;

    private readonly Action applyAction;
    private readonly Action removeAction;

    public bool IsFinished => remainingTime <= 0f;

    public TimedBuff(
        float duration,
        Action applyAction,
        Action removeAction)
    {
        remainingTime = duration;

        this.applyAction = applyAction;
        this.removeAction = removeAction;
    }

    public void Apply()
    {
        applyAction?.Invoke();
    }

    public void Update(float deltaTime)
    {
        remainingTime -= deltaTime;
    }

    public void Remove()
    {
        removeAction?.Invoke();
    }
}