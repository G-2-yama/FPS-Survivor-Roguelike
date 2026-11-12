using System;
using System.Linq;
using EnemyBalance;
using Xunit;

namespace BalanceSim.Tests;

public class EnemySpawnPreviewTests
{
    private static EnemySpawnPreview.Phase Phase(double start, int minimum, double interval) =>
        new() { Start = start, Minimum = minimum, Interval = interval };

    [Fact]
    public void StartsWithMinimumThenAddsAtTheConfiguredFrequency()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 10, 0.5) }, 2, 100, 0);
        Assert.Equal(10, result.At(0).Alive, 6);
        Assert.Equal(10, result.At(0.49).Alive, 6);
        Assert.Equal(11, result.At(0.5).Alive, 6);
        Assert.Equal(14, result.At(2).Alive, 6);
        Assert.Equal(14, result.Spawned, 6);
    }

    [Fact]
    public void LowerMinimumDoesNotDeleteEnemiesFromPreviousPhase()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 20, 10), Phase(2, 0, 1) }, 4, 100, 0);
        Assert.Equal(20, result.At(1.99).Alive, 6);
        Assert.Equal(21, result.At(2).Alive, 6);
        Assert.Equal(23, result.At(4).Alive, 6);
        Assert.Equal(1, result.At(2).PhaseIndex);
    }

    [Fact]
    public void PhaseBoundaryCancelsTheOldSpawnWaitAndFillsNewMinimum()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 10, 100), Phase(5, 40, 2) }, 8, 100, 0);
        Assert.Equal(10, result.At(4.99).Alive, 6);
        Assert.Equal(40, result.At(5).Alive, 6);
        Assert.Equal(41, result.At(7).Alive, 6);
    }

    [Fact]
    public void CoincidentSpawnAndBoundaryDoNotSpawnTheOldPhaseAgain()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 10, 1), Phase(1, 0, 10) }, 2, 100, 0);
        Assert.Equal(11, result.Spawned, 6);
        Assert.Equal(11, result.At(1).Alive, 6);
    }

    [Fact]
    public void CapacityCapsSurvivorsAndCountsReplacementSpawns()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 10, 1) }, 2, 5, 0);
        Assert.Equal(5, result.Peak, 6);
        Assert.Equal(20, result.Spawned, 6);
        Assert.Equal(15, result.Replaced, 6);
    }

    [Fact]
    public void DeathsReduceTheCurveAndMinimumIsRefilledAtSpawnEvents()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 10, 1) }, 2, 100, 4);
        Assert.Equal(8, result.At(0.5).Alive, 6);
        Assert.Equal(10, result.At(1).Alive, 6);
        Assert.Equal(18, result.Spawned, 6);
        Assert.Equal(0, result.Replaced, 6);
    }

    [Fact]
    public void EmptySceneStillSpawnsOneWithZeroMinimum()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 0, 1) }, 2, 100, 0);
        Assert.Equal(1, result.At(0).Alive, 6);
        Assert.Equal(3, result.At(2).Alive, 6);
    }

    [Fact]
    public void FinalPhaseContinuesForTheWholeEndlessPreview()
    {
        var result = EnemySpawnPreview.Run(new[] { Phase(0, 1, 10), Phase(5, 5, 1) }, 100, 1000, 0);
        Assert.Equal(100, result.At(100).Alive, 6);
        Assert.Equal(1, result.At(100).PhaseIndex);
    }

    [Fact]
    public void AnyNumberOfUnsortedPhasesIsSupported()
    {
        var phases = Enumerable.Range(0, 80).Reverse().Select(i => Phase(i * 2, i * 10, 100));
        var result = EnemySpawnPreview.Run(phases, 160, 10000, 0);
        Assert.Equal(79, result.At(160).PhaseIndex);
        Assert.Equal(790, result.At(160).Alive, 6);
    }

    [Fact]
    public void InvalidOrAmbiguousSettingsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => EnemySpawnPreview.Run(new[] { Phase(0, 1, 0) }, 10, 10, 0));
        Assert.Throws<ArgumentException>(() => EnemySpawnPreview.Run(new[] { Phase(0, 1, double.NaN) }, 10, 10, 0));
        Assert.Throws<ArgumentException>(() => EnemySpawnPreview.Run(new[] { Phase(0, -1, 1) }, 10, 10, 0));
        Assert.Throws<ArgumentException>(() => EnemySpawnPreview.Run(new[] { Phase(0, 1, 1), Phase(0, 2, 1) }, 10, 10, 0));
        Assert.Throws<ArgumentException>(() => EnemySpawnPreview.Run(new[] { Phase(0, 1, 1) }, 10, 10, -1));
        Assert.Throws<ArgumentException>(() => EnemySpawnPreview.Run(new[] { Phase(0, 1, 1) }, 10, 0, 0));
    }
}
