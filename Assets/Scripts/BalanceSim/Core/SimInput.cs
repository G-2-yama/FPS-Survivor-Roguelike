using System;
using System.Collections.Generic;

namespace BalanceSim
{
    [Serializable]
    public class SimInput
    {
        public int runCount;
        public int seed;
        public SimOptions options = new();
        public SimPolicyRef choicePolicy = new();
        public float timeLimit;
        public SimPlayerDef player = new();
        public SimTerrainDef terrain = new();
        public List<SimGeneratorDef> generators = new();
        public List<SimEnemyDef> enemies = new();
        public List<SimPickupDef> pickups = new();
        public List<SimUpgradeDef> upgrades = new();
        public List<SimWeaponDef> weapons = new();
        public List<SimProjectileDef> projectiles = new();
        public List<SimItemDef> items = new();
    }

    public enum SimFacingMode
    {
        MoveDirection,
        NearestEnemy,
    }

    [Serializable]
    public class SimOptions
    {
        public float timeStep = 0.02f;
        public float sampleInterval = 1f;
        public SimFacingMode facing;
        public float escapeRadius = 30f;
        public float escapeLookahead = 15f;
        public float escapeDeadEndWeight = 1f;
        public float escapeTurnWeight = 0.2f;
        public float aimError = 2f;
        public float fireRange = 60f;
    }

    [Serializable]
    public class SimPolicyRef
    {
        public string typeName;
        public string json;
    }

    [Serializable]
    public class SimPlayerDef
    {
        public float x;
        public float z;
        public float radius;
        public int maxHp;
        public float runSpeed;
        public float levelUpRequiredExp;
        public float levelUpRate;
        public float eyeHeight;
        public float pivotHeight;
        public List<string> initialWeapons = new();
    }

    [Serializable]
    public class SimTerrainDef
    {
        public float originX;
        public float originZ;
        public float tileSize;
        public int gridSize;
        public float cellSize;
        public int cells;
        public string blockedBits;
    }

    [Serializable]
    public class SimGeneratorDef
    {
        public string name;
        public float spawnRadius;
        public int maxSpawnTry;
        public int maxEnemyCount;
        public List<SimSpawnDirection> directions = new();
        public List<SimPhase> phases = new();
    }

    [Serializable]
    public class SimSpawnDirection
    {
        public float angle;
        public float weight;
    }

    [Serializable]
    public class SimPhase
    {
        public string name;
        public float startTime;
        public int minSpawnCount;
        public float spawnInterval;
        public List<SimSpawnEntry> enemies = new();
    }

    [Serializable]
    public class SimSpawnEntry
    {
        public string prefabName;
        public int spawnWeight;
    }

    public enum SimMovementKind
    {
        Stop,
        Toward,
        Item,
        Orbit,
        Roll,
        Jump,
    }

    [Serializable]
    public class SimMovementDef
    {
        public SimMovementKind kind;
        public float chaseSpeed;
        public float acceleration;
        public float maxSpeed;
        public float orbitRadius;
        public float orbitAngularSpeed;
        public float moveForce;
        public float forwardForce;
        public float upwardForce;
    }

    [Serializable]
    public class SimEnemyDef
    {
        public string name;
        public bool hasEnemyComponent;
        public int maxHp;
        public int damageRange;
        public float knockbackResistance;
        public float knockbackDuration;
        public float engageDistance;
        public SimMovementDef chase = new();
        public SimMovementDef combat = new();
        public float radius;
        public float linearDamping;
        public bool useGravity;
        public int touchDamage;
        public SimAttackDef attack = new();
        public SimAreaDef contactArea = new();
        public float deathDelay;
        public float destroyDamageDamping;
        public string deathExplosion;
        public List<SimDropEntry> drops = new();
    }

    [Serializable]
    public class SimDropEntry
    {
        public string pickupName;
        public int weight;
    }

    [Serializable]
    public class SimAttackDef
    {
        public int shotCount;
        public float interval;
        public float firstShotRate;
        public float attackInterval;
        public int attackPower;
        public float shotPointX;
        public float shotPointZ;
        public SimShotDef shot = new();
    }

    public enum SimShotKind
    {
        None,
        Projectile,
        Area,
    }

    [Serializable]
    public class SimShotDef
    {
        public SimShotKind kind;
        public float speed;
        public float radius;
        public int damageRange;
        public int hp;
        public bool hasBrain;
        public float engageDistance;
        public SimMovementDef chase = new();
        public SimMovementDef combat = new();
        public float linearDamping;
        public SimAreaDef area = new();
    }

    [Serializable]
    public class SimAreaDef
    {
        public bool enabled;
        public int damage;
        public float lifetime;
        public float centerX;
        public float centerZ;
        public float halfX;
        public float halfZ;
    }

    public enum SimPickupKind
    {
        Exp,
        Heal,
        SpeedUp,
    }

    [Serializable]
    public class SimPickupDef
    {
        public string name;
        public SimPickupKind kind;
        public float amount;
        public float duration;
        public float radius;
        public string rangeKey;
        public float baseRange;
        public SimMovementDef chase = new();
        public SimMovementDef combat = new();
    }

    public enum SimUpgradeKind
    {
        WeaponLevelUp,
        WeaponUnlock,
        GetItem,
        ItemLevelUp,
    }

    [Serializable]
    public class SimUpgradeDef
    {
        public string name;
        public SimUpgradeKind kind;
        public int weight;
        public int slot;
        public string weapon;
        public string item;
        public int itemIndex;
    }

    public enum SimWeaponType
    {
        Main,
        Ability,
        AutoWeapon,
    }

    [Serializable]
    public class SimWeaponDef
    {
        public string name;
        public SimWeaponType type;
        public string next;
        public bool autoFire;
        public bool autoReload;
        public bool fullAuto;
        public int damage;
        public float knockback;
        public float spreadAngle;
        public float fireInterval;
        public int burstCount;
        public float burstInterval;
        public int magazineSize;
        public float reloadTime;
        public SimFireModeDef fireMode = new();
    }

    public enum SimFireModeKind
    {
        None,
        HitScan,
        Chain,
        Projectile,
        Surrounded,
        Area,
        AutoTarget,
        Step,
    }

    [Serializable]
    public class SimFireModeDef
    {
        public string name;
        public SimFireModeKind kind;
        public int targetTeam;
        public float maxRange;
        public float hitRadius;
        public float chainRange;
        public int chainCount;
        public string projectile;
        public float offsetX;
        public float offsetZ;
        public bool tracking;
        public int projectileCount;
        public float areaSizeX;
        public float areaSizeZ;
        public float spawnHeight;
        public float searchRadius;
        public float stepInterval;
    }

    public enum SimDamageKind
    {
        Object,
        Area,
        Generator,
    }

    public enum SimAreaMode
    {
        OnEnterOnce,
        Interval,
    }

    public enum SimProjectileMoveKind
    {
        None,
        Straight,
        Tracking,
        Boomerang,
        Parabola,
        Orbit,
        Rotate,
        Fall,
    }

    [Serializable]
    public class SimProjectileDef
    {
        public string name;
        public SimDamageKind kind;
        public int targetTeam;
        public int damage;
        public float knockback;
        public float lifetime;
        public SimAreaMode areaMode;
        public float interval;
        public bool collides;
        public SimShapeDef shape = new();
        public SimProjectileMoveDef move = new();
        public List<string> explosions = new();
        public List<SimScaleStep> scaleSteps = new();
        public float defaultScale = 1f;
    }

    [Serializable]
    public class SimShapeDef
    {
        public bool circle;
        public float radius;
        public float centerX;
        public float centerZ;
        public float halfX;
        public float halfZ;
    }

    [Serializable]
    public class SimProjectileMoveDef
    {
        public SimProjectileMoveKind kind;
        public float speed;
        public float trackingRange;
        public float rotateSpeed;
        public float width;
        public float length;
        public float duration;
        public float gravity;
        public float upwardSpeed;
        public float radius;
    }

    [Serializable]
    public class SimScaleStep
    {
        public int damageThreshold;
        public float scale;
    }

    public enum SimItemKind
    {
        Unknown,
        HealthUp,
        ExpCatcher,
        DamageUp,
        KnockbackUp,
        SyncWeapon,
    }

    [Serializable]
    public class SimItemDef
    {
        public string name;
        public SimItemKind kind;
        public float amount;
        public string rangeKey;
        public string next;
    }
}
