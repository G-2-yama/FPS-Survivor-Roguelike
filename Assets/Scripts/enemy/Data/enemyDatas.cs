using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Status")]
    [Min(1)] public int MaxHp = 5;

    [Header("Movement")]
    [Min(0f)] public float ChaseSpeed = 3f;
    [Min(0f)] public float OrbitRadius = 3f;
    [Min(0f)] public float OrbitAngularSpeed = 180f;
    [Min(0f)] public float EngageDistance = 5f;

    [Header("Combat")]
    [Min(0)] public int AttackPower = 1;
    [Min(0.01f)] public float AttackInterval = 3f;
    public AttackPattern AttackPattern;
}
