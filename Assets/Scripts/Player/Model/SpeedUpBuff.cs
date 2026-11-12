/// <summary>速度強化を一度だけ適用し、再取得では効果時間を加算する</summary>
public sealed class SpeedUpBuff : TimedBuff
{
    private readonly PlayerStats stats;
    private readonly float duration;

    public SpeedUpBuff(PlayerStats stats, float amount, float duration)
        : base(duration,
            () =>
            {
                stats.AddRunSpeed(amount);
                stats.AddWalkSpeed(amount);
                stats.AddJumpForce(amount);
            },
            () =>
            {
                stats.AddRunSpeed(-amount);
                stats.AddWalkSpeed(-amount);
                stats.AddJumpForce(-amount);
            })
    {
        this.stats = stats;
        this.duration = duration;
    }

    public override bool TryMerge(TimedBuff other)
    {
        if (!(other is SpeedUpBuff speedUp) || !ReferenceEquals(stats, speedUp.stats)) return false;

        ExtendDuration(speedUp.duration);
        return true;
    }
}