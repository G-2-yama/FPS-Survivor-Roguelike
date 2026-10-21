using System.Reflection;
using BalanceSim;

namespace BalanceSim.Tests;

public class KnockbackTests
{
    private static (SimRun Run, EnemyAgent Enemy) CreateEnemy(float cooldown = 1f)
    {
        var input = TestInputs.Minimal();
        var run = new SimRun(new SimData(input), new RandomChoicePolicy(), 1);
        var def = TestInputs.HarmlessEnemy("target");
        def.maxHp = 100;
        def.knockbackDuration = 0.2f;
        def.knockbackCooldown = cooldown;
        var enemy = new EnemyAgent { Def = def, Hp = 100, Position = new Vec2(0f, 2f), InCombat = true };
        return (run, enemy);
    }

    private static void UpdateEnemy(SimRun run, EnemyAgent enemy, float time, float dt)
    {
        typeof(SimRun).GetMethod("UpdateEnemy", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(run, new object[] { enemy, time, dt });
    }

    [Fact]
    public void RepeatedHitsDealDamageWithoutExtendingKnockbackOrInterruptingRecoveredAttack()
    {
        var (run, enemy) = CreateEnemy();
        run.DamageEnemy(enemy, 1, 5f, 0f);
        UpdateEnemy(run, enemy, 0.1f, 0.1f);
        run.DamageEnemy(enemy, 1, 10f, 0.1f);
        Assert.Equal(0.1f, enemy.KnockbackRemaining);
        UpdateEnemy(run, enemy, 0.2f, 0.1f);
        Assert.False(enemy.InKnockback);
        Assert.Equal(1.2f, enemy.NextKnockbackTime);

        enemy.InCombat = true;
        enemy.AttackRunning = true;
        run.DamageEnemy(enemy, 1, 5f, 0.3f);
        run.DamageEnemy(enemy, 1, 5f, 1.19f);
        Assert.False(enemy.InKnockback);
        Assert.True(enemy.InCombat);
        Assert.True(enemy.AttackRunning);
        Assert.Equal(96, enemy.Hp);
        Assert.Equal(1.2f, enemy.NextKnockbackTime);

        run.DamageEnemy(enemy, 1, 5f, 1.2f);
        Assert.True(enemy.InKnockback);
        Assert.False(enemy.AttackRunning);
    }

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(5f, 1f)]
    public void ZeroEffectiveForceDoesNotInterruptAttack(float force, float resistance)
    {
        var (run, enemy) = CreateEnemy();
        enemy.Def.knockbackResistance = resistance;
        enemy.AttackRunning = true;
        run.DamageEnemy(enemy, 1, force, 0f);
        Assert.Equal(99, enemy.Hp);
        Assert.False(enemy.InKnockback);
        Assert.True(enemy.InCombat);
        Assert.True(enemy.AttackRunning);
    }

    [Theory]
    [InlineData(0.1f, 0f, 1f)]
    [InlineData(0.4f, 3f, 2.5f)]
    [InlineData(0.05f, 40f, 3f)]
    [InlineData(0.45f, 1f, 1.5f)]
    [InlineData(1f, 2f, 3.5f)]
    public void EnemyCanResumeAndCompleteAnAttackWhileTakingHitsDuringCooldown(float interval, float firstShotRate, float cooldown)
    {
        var (run, enemy) = CreateEnemy(cooldown);
        enemy.Def.attack = new SimAttackDef
        {
            shotCount = 1,
            interval = interval,
            firstShotRate = firstShotRate,
            attackInterval = 3f,
            shot = new SimShotDef { kind = SimShotKind.Projectile, hp = 1, speed = 5f },
        };
        run.DamageEnemy(enemy, 1, 5f, 0f);
        UpdateEnemy(run, enemy, 0.2f, 0.2f);
        UpdateEnemy(run, enemy, 0.3f, 0.1f);
        UpdateEnemy(run, enemy, 0.4f, 0.1f);
        Assert.True(enemy.AttackRunning);

        int hitCount = 1;
        for (int step = 5; enemy.ShotsFired == 0 && step * 0.1f < 0.2f + cooldown; step++)
        {
            float time = step * 0.1f;
            run.DamageEnemy(enemy, 1, 5f, time);
            hitCount++;
            UpdateEnemy(run, enemy, time, 0.1f);
        }
        Assert.Equal(1, enemy.ShotsFired);
        Assert.Equal(100 - hitCount, enemy.Hp);
        Assert.False(enemy.InKnockback);
    }

    [Fact]
    public void ZeroCooldownAllowsAnotherKnockbackImmediatelyAfterRecovery()
    {
        var (run, enemy) = CreateEnemy(0f);
        run.DamageEnemy(enemy, 1, 5f, 0f);
        UpdateEnemy(run, enemy, 0.2f, 0.2f);
        run.DamageEnemy(enemy, 1, 5f, 0.2f);
        Assert.True(enemy.InKnockback);
    }

    [Fact]
    public void CooldownBelongsToEachEnemy()
    {
        var (run, first) = CreateEnemy();
        var second = new EnemyAgent { Def = first.Def, Hp = 100, Position = first.Position };
        run.DamageEnemy(first, 1, 5f, 0f);
        UpdateEnemy(run, first, 0.2f, 0.2f);
        run.DamageEnemy(second, 1, 5f, 0.3f);
        Assert.True(second.InKnockback);
        Assert.False(first.InKnockback);
    }
}
