using BalanceSim;

namespace BalanceSim.Tests;

internal static class TestInputs
{
    public static SimInput Minimal(float timeLimit = 5f)
    {
        return new SimInput
        {
            runCount = 1,
            seed = 1,
            timeLimit = timeLimit,
            player = new SimPlayerDef
            {
                radius = 0.5f,
                maxHp = 1000,
                runSpeed = 10f,
                levelUpRequiredExp = 50f,
                levelUpRate = 1.08f,
                initialWeapons = new List<string> { "HG_00", "", "", "", "", "" },
            },
            terrain = new SimTerrainDef { tileSize = 30f, gridSize = 7, cellSize = 0.5f },
        };
    }

    public static SimEnemyDef HarmlessEnemy(string name)
    {
        return new SimEnemyDef
        {
            name = name,
            hasEnemyComponent = true,
            maxHp = 10,
            damageRange = 1,
            engageDistance = 5f,
            radius = 0.5f,
        };
    }

    public static SimWeaponDef Weapon(string name, SimFireModeDef fireMode, int damage, int magazineSize = 1, float reloadTime = 100f)
    {
        return new SimWeaponDef
        {
            name = name,
            type = SimWeaponType.Main,
            autoReload = true,
            damage = damage,
            fireInterval = 0.1f,
            burstCount = 1,
            magazineSize = magazineSize,
            reloadTime = reloadTime,
            fireMode = fireMode,
        };
    }

    public static SimFireModeDef HitScan()
    {
        return new SimFireModeDef { name = "hitscan", kind = SimFireModeKind.HitScan, targetTeam = 28, maxRange = 60f, hitRadius = 0.1f };
    }

    public static SimGeneratorDef Generator(string prefabName, int minSpawnCount, float spawnInterval, int maxEnemyCount = 1500)
    {
        return new SimGeneratorDef
        {
            name = "gen",
            spawnRadius = 80f,
            maxSpawnTry = 5,
            maxEnemyCount = maxEnemyCount,
            directions = new List<SimSpawnDirection> { new() { angle = 180f, weight = 1f } },
            phases = new List<SimPhase>
            {
                new()
                {
                    name = "p0",
                    startTime = 0f,
                    minSpawnCount = minSpawnCount,
                    spawnInterval = spawnInterval,
                    enemies = new List<SimSpawnEntry> { new() { prefabName = prefabName, spawnWeight = 1 } },
                },
            },
        };
    }
}
