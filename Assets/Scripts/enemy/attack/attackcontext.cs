using UnityEngine;

public sealed class AttackContext
{
    private readonly EnemyAttackController owner;

    public EnemyAttackController Owner => owner;
    public Transform Target => owner.CurrentTarget;
    public Transform ShotPoint => owner.ShotPoint;
    public ProjectileLauncher Launcher => owner.Launcher;
    public int AttackPower => owner.AttackPower;

    public AttackContext(EnemyAttackController owner)
    {
        this.owner = owner;
    }
}

