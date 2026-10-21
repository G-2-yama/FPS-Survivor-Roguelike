using System.Reflection;
using BalanceSim;
using EnemyBalance;

namespace BalanceSim.Tests;

public class PhaseDpsTests
{
    [Theory]
    [InlineData(250, 1)]
    [InlineData(1000, 1)]
    [InlineData(1210, 1.1)]
    [InlineData(1440, 1.2)]
    [InlineData(1690, 1.3)]
    [InlineData(10000, 1.3)]
    public void MeasuresNormalPhaseAndClampsSquareRootMultiplier(int damage, double expected)
    {
        var tracker = new PhaseDpsTracker();
        tracker.BeginPhase(false, 30);
        tracker.RecordDamage(damage);
        tracker.BeginPhase(true, 40);
        Assert.Equal(damage / 10.0, tracker.PreviousNormalDps);
        Assert.Equal(expected, tracker.GetHpMultiplier(100, 1, 1.3), 10);
    }

    [Fact]
    public void LegacyBoundsCannotReduceHpAndEachPhaseCanUseASmallerCap()
    {
        var tracker = new PhaseDpsTracker();
        tracker.BeginPhase(false, 0);
        tracker.RecordDamage(250);
        tracker.BeginPhase(true, 10);
        Assert.Equal(1, tracker.GetHpMultiplier(100, 0.8, 0.9));
        Assert.Equal(1.2, tracker.GetHpMultiplier(1, 1, 1.2));
    }

    [Fact]
    public void ExDamageIsIgnoredAndNextNormalPhaseStartsFresh()
    {
        var tracker = new PhaseDpsTracker();
        tracker.BeginPhase(false, 0);
        tracker.RecordDamage(1000);
        tracker.BeginPhase(true, 10);
        tracker.RecordDamage(100000);
        Assert.Equal(100, tracker.PreviousNormalDps);
        tracker.BeginPhase(true, 12);
        Assert.Equal(100, tracker.PreviousNormalDps);
        tracker.BeginPhase(false, 14);
        tracker.RecordDamage(100);
        tracker.BeginPhase(true, 24);
        Assert.Equal(10, tracker.PreviousNormalDps);
    }

    [Fact]
    public void NoValidMeasurementFallsBackToOneWithinConfiguredBounds()
    {
        var tracker = new PhaseDpsTracker();
        tracker.RecordDamage(1000);
        tracker.BeginPhase(false, 0);
        tracker.RecordDamage(-100);
        tracker.BeginPhase(true, 10);
        Assert.False(tracker.HasPreviousNormalDps);
        Assert.Equal(1, tracker.GetHpMultiplier(100, 0.8, 1.8));
        Assert.Equal(2, tracker.GetHpMultiplier(100, 2, 3));
        tracker.BeginPhase(false, 10);
        tracker.RecordDamage(100);
        tracker.BeginPhase(true, 10);
        Assert.False(tracker.HasPreviousNormalDps);
    }

    [Fact]
    public void HpRoundingCannotProduceZeroOrOverflow()
    {
        Assert.Equal(1, PhaseDpsTracker.ScaleHp(1, 0.01));
        Assert.Equal(5, PhaseDpsTracker.ScaleHp(3, 1.5));
        Assert.Equal(int.MaxValue, PhaseDpsTracker.ScaleHp(int.MaxValue, 2));
    }

    [Fact]
    public void SpawnedExHpUsesActualDamageAndIsFixedWithoutChangingDefinitions()
    {
        var (run, generator, normalDef, exDef) = CreateRun();
        Update(run, generator, 0);
        EnemyAgent normal = generator.ActiveEnemies.Single();
        run.DamageEnemy(normal, 1000, 0, 5); // HP100を超えた900は計測しない。
        Update(run, generator, 10);
        EnemyAgent firstEx = generator.ActiveEnemies.Last();
        Assert.Equal(10, generator.PhaseDps.PreviousNormalDps);
        Assert.Equal(1.3f, generator.HpMultiplier);
        Assert.Equal(260, firstEx.Hp);
        run.DamageEnemy(firstEx, 100, 0, 10.5f);
        Update(run, generator, 11);
        Assert.Equal(260, generator.ActiveEnemies.Last().Hp);
        Update(run, generator, 12);
        Assert.Equal(100, generator.ActiveEnemies.Last().Hp);
        Assert.Equal(160, firstEx.Hp); // 通常Phase復帰時も既存個体を再補正しない。
        Assert.Equal(100, normalDef.maxHp);
        Assert.Equal(200, exDef.maxHp);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ExEnemiesAndItemBoxesDoNotContributeToNormalDps(bool exEnemy, bool obstacle)
    {
        var (run, generator, normalDef, _) = CreateRun();
        normalDef.isExEnemy = exEnemy;
        normalDef.isPlayerObstacle = obstacle;
        Update(run, generator, 0);
        run.DamageEnemy(generator.ActiveEnemies.Single(), 50, 0, 5);
        Update(run, generator, 10);
        Assert.False(generator.PhaseDps.HasPreviousNormalDps);
        Assert.Equal(1f, generator.HpMultiplier);
    }

    [Fact]
    public void DamageAtExBoundaryIsNotAddedToPreviousNormalPhase()
    {
        var (run, generator, _, _) = CreateRun();
        Update(run, generator, 0);
        run.DamageEnemy(generator.ActiveEnemies.Single(), 50, 0, 10);
        Assert.False(generator.PhaseDps.HasPreviousNormalDps);
        Assert.True(generator.CurrentPhase.isExPhase);
    }

    private static (SimRun, GeneratorState, SimEnemyDef, SimEnemyDef) CreateRun()
    {
        SimInput input = TestInputs.Minimal(15);
        SimEnemyDef normal = TestInputs.HarmlessEnemy("normal");
        normal.maxHp = 100;
        SimEnemyDef ex = TestInputs.HarmlessEnemy("ex");
        ex.maxHp = 200;
        ex.isExEnemy = true;
        input.enemies.Add(normal);
        input.enemies.Add(ex);
        var generator = TestInputs.Generator("normal", 1, 10);
        generator.phases.Add(new SimPhase
        {
            name = "ex", startTime = 10, minSpawnCount = 1, spawnInterval = 0.8f,
            isExPhase = true, referenceDps = 2.5f, minHpMultiplier = 1f, maxHpMultiplier = 1.3f,
            enemies = new List<SimSpawnEntry> { new() { prefabName = "ex", spawnWeight = 1 } },
        });
        generator.phases.Add(new SimPhase
        {
            name = "next normal", startTime = 12, minSpawnCount = 1, spawnInterval = 1,
            enemies = new List<SimSpawnEntry> { new() { prefabName = "normal", spawnWeight = 1 } },
        });
        input.generators.Add(generator);
        var run = new SimRun(new SimData(input), new RandomChoicePolicy(), 1);
        var states = (List<GeneratorState>)typeof(SimRun).GetField("_generators", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(run);
        return (run, states.Single(), normal, ex);
    }

    private static void Update(SimRun run, GeneratorState generator, float time)
    {
        typeof(SimRun).GetMethod("UpdateGenerator", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(run, new object[] { generator, time });
    }
}
