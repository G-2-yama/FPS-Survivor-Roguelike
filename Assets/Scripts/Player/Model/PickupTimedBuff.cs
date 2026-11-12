/// <summary>SOから効果をコピーし、プレイヤーごとに期限と解除処理を保持する。</summary>
public sealed class PickupTimedBuff : TimedBuff
{
    private readonly PlayerStats stats;
    private readonly PickupEffectDefinition definition;
    private readonly float duration;
    private readonly BuffReacquireMode reacquireMode;
    private readonly PickupEffectEntry[] effects;
    public override PickupEffectDefinition Definition => definition;

    public PickupTimedBuff(PlayerStats stats, PickupEffectDefinition definition)
        : base(definition.Duration, null, null)
    {
        this.stats = stats;
        this.definition = definition;
        duration = definition.Duration;
        reacquireMode = definition.ReacquireMode;
        effects = new PickupEffectEntry[definition.Effects.Count];
        for (int i = 0; i < effects.Length; i++) effects[i] = definition.Effects[i];
    }

    public override void Apply()
    {
        foreach (var effect in effects) PickupEffectApplier.ApplyStat(stats, effect, 1f);
    }

    public override void Remove()
    {
        foreach (var effect in effects) PickupEffectApplier.ApplyStat(stats, effect, -1f);
    }

    public override bool TryMerge(TimedBuff other)
    {
        if (!(other is PickupTimedBuff incoming) || incoming.definition != definition || !ReferenceEquals(incoming.stats, stats)) return false;
        switch (reacquireMode)
        {
            case BuffReacquireMode.Extend: ExtendDuration(incoming.duration); return true;
            case BuffReacquireMode.Refresh: ResetDuration(incoming.duration); return true;
            default: return false;
        }
    }
}