using BalanceSim;

namespace BalanceSim.Tests;

public class ExperienceRequirementsTests
{
    [Fact]
    public void LegacyFormulaAndConstantRateKeepTheirTotals()
    {
        Assert.Equal(700d, ExperienceRequirements.Total(1, 100f, 2f, null, 1, 4));
        Assert.Equal(4900d, ExperienceRequirements.Total(1, 100f, 1f, null, 1, 50));
        Assert.Equal(0d, ExperienceRequirements.Total(1, 100f, 2f, null, 50, 50));
    }

    [Fact]
    public void EveryVertexCanDecreaseAndLastVertexContinuesBeyondTable()
    {
        float[] levels = { 10f, 50f, 5f };
        Assert.Equal(50d, ExperienceRequirements.At(3, 999f, 2f, levels, 4));
        Assert.Equal(5d, ExperienceRequirements.At(3, 999f, 2f, levels, 100));
        Assert.Equal(60d, ExperienceRequirements.Total(3, 999f, 2f, levels, 3, 5));
        Assert.Equal(65d, ExperienceRequirements.Total(3, 999f, 2f, levels, 3, 6));
        Assert.Equal(10d, ExperienceRequirements.Total(3, 999f, 2f, levels, 5, 7));
    }

    [Fact]
    public void SmallIntervalsAfterLargeRequirementsKeepTheirPrecision()
    {
        float[] levels = { float.MaxValue, 1f, 2f };
        Assert.Equal(3d, ExperienceRequirements.Total(1, 10f, 2f, levels, 2, 4));
        Assert.Equal(2d * (int.MaxValue - 3d),
            ExperienceRequirements.Total(1, 10f, 2f, levels, 3, int.MaxValue));
    }

    [Fact]
    public void SimulatorUsesEachEditedThresholdForMultipleLevelUps()
    {
        var input = new SimInput();
        input.player.initialLevel = 3;
        input.player.requiredExpByLevel = new[] { 10f, 50f, 5f };
        var progression = new Progression(input, new PlayerState(100), new RandomChoicePolicy(), new Random(1));
        progression.AddExp(67f, 1f);
        Assert.Equal(6, progression.Level);
        Assert.Equal(2f, progression.Exp);
        Assert.Equal(5f, progression.RequiredExp);
        Assert.Equal(new[] { 4, 5, 6 }, progression.LevelUps.Select(record => record.level));
    }
}
