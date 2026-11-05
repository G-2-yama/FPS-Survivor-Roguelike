using EnemyBalance;

namespace BalanceSim.Tests;

public class EnemyCycleTimelineTests
{
    [Fact]
    public void NormalIntervalAndRushUseExactBoundaries()
    {
        var state = new EnemyCycleTimeline(10, 30, 25);
        state.Advance(9, false);
        Assert.False(state.HasStarted);
        state.Advance(10, false);
        Assert.True(state.HasStarted);
        state.Advance(39.99, false);
        Assert.Equal(EnemyCycleStage.Normal, state.Stage);
        state.Advance(40, false);
        Assert.Equal(EnemyCycleStage.Boss, state.Stage);
        state.Advance(49.99, true);
        Assert.Equal(EnemyCycleStage.Boss, state.Stage);
        state.Advance(50, false);
        Assert.Equal(EnemyCycleStage.Rush, state.Stage);
        state.Advance(74.99, false);
        Assert.False(state.IsComplete);
        state.Advance(75, false);
        Assert.True(state.IsComplete);
    }

    [Fact]
    public void ZeroDurationsCompleteWithoutWaitingForDefeat()
    {
        var state = new EnemyCycleTimeline(0, 0, 0, 0);
        state.Advance(100, false);
        Assert.True(state.IsComplete);
    }

    [Fact]
    public void SharedDefinitionDoesNotShareProgress()
    {
        var first = new EnemyCycleTimeline(0, 10, 20);
        var second = new EnemyCycleTimeline(50, 10, 20);
        first.Advance(20, false);
        second.Advance(51, false);
        Assert.Equal(EnemyCycleStage.Rush, first.Stage);
        Assert.Equal(EnemyCycleStage.Normal, second.Stage);
        first.Advance(40, false);
        Assert.True(first.IsComplete);
        Assert.False(second.IsComplete);
    }

    [Fact]
    public void DelayedTickDoesNotMoveScheduleOrWaitForBoss()
    {
        var first = new EnemyCycleTimeline(0, 30, 25, 10);
        first.Advance(65.016, false);
        Assert.True(first.IsComplete);
        var next = new EnemyCycleTimeline(65, 30, 25, 10);
        next.Advance(95, false);
        Assert.Equal(EnemyCycleStage.Boss, next.Stage);
        next.Advance(105, false);
        Assert.Equal(EnemyCycleStage.Rush, next.Stage);
        next.Advance(130, false);
        Assert.True(next.IsComplete);
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(0, -1, 1)]
    [InlineData(0, 1, -1)]
    public void RejectsInvalidDurations(double start, double normal, double rush)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyCycleTimeline(start, normal, rush));
    }
}
