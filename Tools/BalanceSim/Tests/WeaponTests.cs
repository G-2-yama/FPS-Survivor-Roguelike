using BalanceSim;

namespace BalanceSim.Tests;

public class WeaponTests
{
    private static SimSeries Series(SimResult result, string name) => result.series.Single(s => s.name == name);

    private static SimInput StandingPlayer(float timeLimit)
    {
        SimInput input = TestInputs.Minimal(timeLimit);
        input.player.runSpeed = 0f;
        input.options.aimError = 0f;
        return input;
    }

    private static SimGeneratorDef AheadGenerator(string name, string prefabName, float distance)
    {
        SimGeneratorDef generator = TestInputs.Generator(prefabName, minSpawnCount: 1, spawnInterval: 1000f);
        generator.name = name;
        generator.spawnRadius = distance;
        generator.directions = new List<SimSpawnDirection> { new() { angle = 0f, weight = 1f } };
        return generator;
    }

    [Fact]
    public void FullAutoHeldWithEmptyMagazineNeverReloads()
    {
        var weapon = new WeaponState();
        weapon.Equip(new SimWeaponDef { fullAuto = true, autoReload = true, magazineSize = 2, burstCount = 1, fireInterval = 0.5f, reloadTime = 1f }, 0f);
        int fired = 0;
        for (int step = 0; step < 400; step++)
        {
            float time = step * 0.25f;
            if (weapon.Phase == WeaponPhase.Idle)
            {
                weapon.OnFire(time);
            }
            weapon.Update(time, 0.25f, pressed: true, _ => fired++);
        }

        Assert.Equal(2, fired);
        Assert.Equal(0, weapon.Ammo);
    }

    [Fact]
    public void AutoWeaponCyclesBurstCooldownAndReload()
    {
        var weapon = new WeaponState();
        weapon.Equip(new SimWeaponDef { type = SimWeaponType.AutoWeapon, autoFire = true, autoReload = true, magazineSize = 2, burstCount = 2, burstInterval = 0f, fireInterval = 0f, reloadTime = 1f }, 0f);
        var fireTimes = new List<float>();
        for (int step = 0; step < 40; step++)
        {
            float time = step * 0.25f;
            weapon.Update(time, 0.25f, pressed: false, _ => fireTimes.Add(time));
        }

        Assert.Equal(new[] { 0f, 0f, 1f, 1f, 2f, 2f, 3f, 3f, 4f, 4f, 5f, 5f, 6f, 6f, 7f, 7f, 8f, 8f, 9f, 9f }, fireTimes);
    }

    [Theory]
    [InlineData(1f / 30f)]
    [InlineData(0.02f)]
    [InlineData(1f / 144f)]
    [InlineData(0.1f)]
    public void FullAutoFireRateDoesNotDependOnTimeStep(float dt)
    {
        var weapon = new WeaponState();
        weapon.Equip(new SimWeaponDef { fullAuto = true, magazineSize = 1000, burstCount = 1, fireInterval = 0.05f, reloadTime = 1f }, 0f);
        int fired = 0;
        for (int step = 0; step * dt < 3f; step++)
        {
            float time = step * dt;
            if (weapon.Phase == WeaponPhase.Idle)
            {
                weapon.OnFire(time);
            }
            weapon.Update(time, dt, pressed: true, _ => fired++);
        }

        Assert.InRange(fired, 59, 61);
    }

    [Theory]
    [InlineData(1f / 30f)]
    [InlineData(0.02f)]
    [InlineData(1f / 144f)]
    [InlineData(0.1f)]
    public void AutoFireBurstRateDoesNotDependOnTimeStep(float dt)
    {
        var weapon = new WeaponState();
        weapon.Equip(new SimWeaponDef { type = SimWeaponType.AutoWeapon, autoFire = true, magazineSize = 1000, burstCount = 3, burstInterval = 0.02f, fireInterval = 0.2f, reloadTime = 1f }, 0f);
        int fired = 0;
        for (int step = 0; step * dt < 3f; step++)
        {
            weapon.Update(step * dt, dt, pressed: false, _ => fired++);
        }

        Assert.InRange(fired, 37, 39);
    }

    [Fact]
    public void HitScanPassesThroughEnemyAmmoAndStopsAtFirstEnemy()
    {
        SimInput input = StandingPlayer(timeLimit: 2f);
        input.weapons.Add(TestInputs.Weapon("HG_00", TestInputs.HitScan(), damage: 10));
        SimEnemyDef ball = TestInputs.HarmlessEnemy("Ball");
        ball.hasEnemyComponent = false;
        input.enemies.Add(ball);
        input.enemies.Add(TestInputs.HarmlessEnemy("Bit"));
        input.enemies.Add(TestInputs.HarmlessEnemy("Bit2"));
        input.generators.Add(AheadGenerator("ball", "Ball", 10f));
        input.generators.Add(AheadGenerator("bit", "Bit", 20f));
        input.generators.Add(AheadGenerator("bit2", "Bit2", 30f));

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());

        Assert.Equal(1f, Series(result, "撃破数").p50[^1]);
        Assert.Equal(0f, Series(result, "bit の生存数").p50[^1]);
        Assert.Equal(1f, Series(result, "bit2 の生存数").p50[^1]);
    }

    [Fact]
    public void DeathExplosionDamagesNeighbour()
    {
        SimInput input = StandingPlayer(timeLimit: 2f);
        input.weapons.Add(TestInputs.Weapon("HG_00", TestInputs.HitScan(), damage: 10));
        input.projectiles.Add(new SimProjectileDef
        {
            name = "Explosion",
            kind = SimDamageKind.Area,
            areaMode = SimAreaMode.OnEnterOnce,
            targetTeam = 28,
            lifetime = 1f,
            shape = new SimShapeDef { halfX = 3f, halfZ = 3f },
        });
        SimEnemyDef front = TestInputs.HarmlessEnemy("Front");
        front.deathExplosion = "Explosion";
        front.destroyDamageDamping = 1f;
        input.enemies.Add(front);
        input.enemies.Add(TestInputs.HarmlessEnemy("Back"));
        input.generators.Add(AheadGenerator("front", "Front", 20f));
        input.generators.Add(AheadGenerator("back", "Back", 22f));

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());

        Assert.Equal(2f, Series(result, "撃破数").p50[^1]);
    }

    [Fact]
    public void StraightBulletHitsEnemy()
    {
        SimInput input = StandingPlayer(timeLimit: 2f);
        input.projectiles.Add(new SimProjectileDef
        {
            name = "Bullet",
            kind = SimDamageKind.Object,
            targetTeam = 28,
            shape = new SimShapeDef { circle = true, radius = 0.16f },
            move = new SimProjectileMoveDef { kind = SimProjectileMoveKind.Straight, speed = 90f },
        });
        var mode = new SimFireModeDef { name = "ar", kind = SimFireModeKind.Projectile, projectile = "Bullet" };
        input.weapons.Add(TestInputs.Weapon("HG_00", mode, damage: 10));
        input.enemies.Add(TestInputs.HarmlessEnemy("Bit"));
        input.generators.Add(AheadGenerator("bit", "Bit", 20f));

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());

        Assert.Equal(1f, Series(result, "撃破数").p50[^1]);
    }

    [Fact]
    public void IntervalAreaDamagesEveryInterval()
    {
        SimInput input = StandingPlayer(timeLimit: 12f);
        input.projectiles.Add(new SimProjectileDef
        {
            name = "Aura",
            kind = SimDamageKind.Area,
            areaMode = SimAreaMode.Interval,
            interval = 3f,
            targetTeam = 28,
            lifetime = 10f,
            shape = new SimShapeDef { circle = true, radius = 25f },
        });
        var mode = new SimFireModeDef { name = "aura", kind = SimFireModeKind.Projectile, projectile = "Aura", tracking = true };
        SimWeaponDef aura = TestInputs.Weapon("HG_00", mode, damage: 1, reloadTime: 10f);
        aura.type = SimWeaponType.AutoWeapon;
        aura.autoFire = true;
        aura.fireInterval = 0f;
        input.weapons.Add(aura);
        SimEnemyDef target = TestInputs.HarmlessEnemy("Bit");
        target.maxHp = 3;
        input.enemies.Add(target);
        input.generators.Add(AheadGenerator("bit", "Bit", 10f));

        SimResult result = Simulator.Run(input, () => new RandomChoicePolicy());
        SimSeries kills = Series(result, "撃破数");

        Assert.Equal(0f, kills.p50[8]);
        Assert.Equal(1f, kills.p50[10]);
    }
}
