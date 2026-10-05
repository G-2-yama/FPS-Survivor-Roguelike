using System.Collections.Generic;

namespace BalanceSim
{
    public enum Presence
    {
        Active,
        Fallen,
        Released,
    }

    public abstract class Body
    {
        public Vec2 Position;
        public Vec2 Velocity;
        public float Height;
        public float VerticalVelocity;
        public Vec2 Facing = Vec2.Forward;
        public bool InCombat;
    }

    public sealed class EnemyAgent : Body
    {
        public SimEnemyDef Def;
        public GeneratorState Generator;
        public Presence Presence = Presence.Active;
        public int Hp;
        public bool InContact;
        public float DeathTime = -1f;
        public float NextAttackTime;
        public bool AttackRunning;
        public int ShotsFired;
        public float NextShotTime;
        public bool IsThreat;
        public bool InKnockback;
        public float KnockbackRemaining;
        public Vec2 KnockbackVelocity;
        public int FinalDamage;
    }

    public sealed class ShotAgent : Body
    {
        public SimShotDef Def;
        public int Damage;
        public int Hp;
        public bool Alive = true;
    }

    public sealed class PlayerProjectile : Body
    {
        public SimProjectileDef Def;
        public int Damage;
        public float Knockback;
        public float Scale = 1f;
        public bool Alive = true;
        public float ExpireTime = float.PositiveInfinity;
        public float Age;
        public bool Tracking;
        public Vec2 LocalPosition;
        public Vec2 LocalFacing = Vec2.Forward;
        public Vec2 Direction;
        public Body Target;
        public float OrbitAngle;
        public readonly HashSet<Body> Inside = new();
        public readonly Dictionary<Body, float> Timers = new();
    }

    public sealed class AreaAgent
    {
        public SimAreaDef Def;
        public Vec2 Origin;
        public Vec2 Forward;
        public float ExpireTime;
        public bool PlayerInside;
    }

    public sealed class PickupAgent : Body
    {
        public SimPickupDef Def;
        public bool Alive = true;
    }

    public sealed class SpeedBuff
    {
        public float Remaining;
        public float Amount;
    }
}
