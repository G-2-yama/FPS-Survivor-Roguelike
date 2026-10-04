using BalanceSim;

namespace BalanceSim.Tests;

public class ResultTests
{
    private static SimInput ChaserInput(int seed)
    {
        SimInput input = TestInputs.Minimal(timeLimit: 20f);
        input.runCount = 5;
        input.seed = seed;
        SimEnemyDef chaser = TestInputs.HarmlessEnemy("Bit");
        chaser.chase = new SimMovementDef { kind = SimMovementKind.Toward, acceleration = 60f, maxSpeed = 15f };
        chaser.touchDamage = 300;
        input.enemies.Add(chaser);
        input.generators.Add(TestInputs.Generator("Bit", minSpawnCount: 50, spawnInterval: 0.2f));
        return input;
    }

    private static void AssertSameResult(SimResult expected, SimResult actual)
    {
        Assert.Equal(expected.message, actual.message);
        Assert.Equal(expected.runCount, actual.runCount);
        Assert.Equal(expected.clearedCount, actual.clearedCount);
        Assert.Equal(expected.deathTimeMean, actual.deathTimeMean);
        Assert.Equal(expected.levelMean, actual.levelMean);
        Assert.Equal(expected.killsMean, actual.killsMean);
        Assert.Equal(expected.series.Select(s => s.mean), actual.series.Select(s => s.mean));
        Assert.Equal(expected.series.Select(s => s.p90), actual.series.Select(s => s.p90));
        Assert.Equal(expected.runs.Select(r => (r.seed, r.cleared, r.endTime, r.level, r.kills)), actual.runs.Select(r => (r.seed, r.cleared, r.endTime, r.level, r.kills)));
    }

    [Fact]
    public void BatchGivesSameResultsAsRunningEachCase()
    {
        SimInput[] inputs = Enumerable.Range(0, 4).Select(ChaserInput).ToArray();
        var batch = new SimResult[inputs.Length];

        Simulator.RunBatch(inputs.Length,
            i => new SimBatchCase { Input = inputs[i], PolicyFactory = () => new RandomChoicePolicy() },
            (i, result) => batch[i] = result);

        for (int i = 0; i < inputs.Length; i++)
        {
            AssertSameResult(Simulator.Run(inputs[i], () => new RandomChoicePolicy()), batch[i]);
        }
    }

    [Fact]
    public void BatchRejectsZeroRunCount()
    {
        SimInput input = ChaserInput(0);
        input.runCount = 0;

        Assert.ThrowsAny<Exception>(() => Simulator.RunBatch(1,
            _ => new SimBatchCase { Input = input, PolicyFactory = () => new RandomChoicePolicy() },
            (_, _) => { }));
    }

    [Fact]
    public void StatsMatchRunSummaries()
    {
        SimResult result = Simulator.Run(ChaserInput(0), () => new RandomChoicePolicy());
        List<SimRunSummary> deaths = result.runs.Where(r => !r.cleared).ToList();

        Assert.NotEmpty(deaths);
        Assert.Equal(result.runs.Count, result.runCount);
        Assert.Equal(result.runs.Count(r => r.cleared), result.clearedCount);
        Assert.Equal(deaths.Average(r => r.endTime), result.deathTimeMean);
        Assert.Equal(result.runs.Average(r => r.level), result.levelMean);
        Assert.Equal(result.runs.Average(r => r.kills), result.killsMean);
    }

    [Fact]
    public void WithoutRunDetailsKeepsStatsAndMeansOnly()
    {
        SimInput detailed = ChaserInput(0);
        SimInput summary = ChaserInput(0);
        summary.options.keepRunDetails = false;

        SimResult full = Simulator.Run(detailed, () => new RandomChoicePolicy());
        SimResult result = Simulator.Run(summary, () => new RandomChoicePolicy());

        Assert.Empty(result.runs);
        Assert.All(result.series, s => Assert.Empty(s.p10));
        Assert.All(result.series, s => Assert.Empty(s.p50));
        Assert.All(result.series, s => Assert.Empty(s.p90));
        Assert.Equal(full.series.Select(s => s.mean), result.series.Select(s => s.mean));
        Assert.Equal(full.message, result.message);
        Assert.Equal(full.clearedCount, result.clearedCount);
        Assert.Equal(full.deathTimeMean, result.deathTimeMean);
    }
}
