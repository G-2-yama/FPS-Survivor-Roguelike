using BalanceSim;

namespace BalanceSim.Tests;

public class SpawnTests
{
    private static SimSeries Series(SimResult result, string name) => result.series.Single(s => s.name == name);

    [Fact]
    public void FillsUpToMinSpawnCountAtStartThenAddsOnePerInterval()
    {
        SimInput input = TestInputs.Minimal(timeLimit: 3f);
        input.enemies.Add(TestInputs.HarmlessEnemy("Bit"));
        input.generators.Add(TestInputs.Generator("Bit", minSpawnCount: 10, spawnInterval: 1f));

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());
        SimSeries listCount = Series(result, "gen のリスト数");

        Assert.Equal(0f, listCount.p50[0]);
        Assert.Equal(10f, listCount.p50[1]);
        Assert.Equal(11f, listCount.p50[2]);
        Assert.Equal(12f, listCount.p50[3]);
    }

    [Fact]
    public void ReplacesFarthestEnemyWhenListIsFull()
    {
        SimInput input = TestInputs.Minimal(timeLimit: 3f);
        input.enemies.Add(TestInputs.HarmlessEnemy("Bit"));
        input.generators.Add(TestInputs.Generator("Bit", minSpawnCount: 10, spawnInterval: 0.5f, maxEnemyCount: 5));

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());

        Assert.All(Series(result, "gen のリスト数").p50.Skip(1), v => Assert.Equal(5f, v));
        Assert.All(Series(result, "gen の生存数").p50.Skip(1), v => Assert.Equal(5f, v));
    }

    [Fact]
    public void EnemiesWithoutEnemyComponentStayInListAfterContact()
    {
        SimInput input = TestInputs.Minimal(timeLimit: 40f);
        SimEnemyDef ball = TestInputs.HarmlessEnemy("Ball");
        ball.hasEnemyComponent = false;
        ball.chase = new SimMovementDef { kind = SimMovementKind.Toward, acceleration = 60f, maxSpeed = 30f };
        ball.combat = ball.chase;
        ball.contactArea = new SimAreaDef { enabled = true, damage = 10, lifetime = 1f, halfX = 1f, halfZ = 1f };
        input.enemies.Add(ball);
        SimGeneratorDef generator = TestInputs.Generator("Ball", minSpawnCount: 1, spawnInterval: 100f);
        generator.spawnRadius = 20f;
        input.generators.Add(generator);

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());
        SimSeries alive = Series(result, "gen の生存数");
        SimSeries list = Series(result, "gen のリスト数");
        SimSeries hp = Series(result, "HP");

        Assert.Equal(0f, alive.p50[^1]);
        Assert.Equal(1f, list.p50[^1]);
        Assert.Equal(990f, hp.p50[^1]);
    }

    [Fact]
    public void SameSeedGivesSameResult()
    {
        SimInput input = TestInputs.Minimal(timeLimit: 10f);
        input.runCount = 4;
        SimEnemyDef chaser = TestInputs.HarmlessEnemy("Bit");
        chaser.chase = new SimMovementDef { kind = SimMovementKind.Toward, acceleration = 15f, maxSpeed = 3f };
        chaser.touchDamage = 10;
        input.enemies.Add(chaser);
        input.generators.Add(TestInputs.Generator("Bit", minSpawnCount: 50, spawnInterval: 0.2f));

        SimResult first = Simulator.Run(input, () => new RandomChoicePolicy());
        SimResult second = Simulator.Run(input, () => new RandomChoicePolicy());

        Assert.Equal(first.series.Select(s => s.mean), second.series.Select(s => s.mean));
    }
}
