using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "PhaseData", menuName = "EnemyGenerator/PhaseData")]
public class PhaseData : ScriptableObject
{
    [SerializeField] private float startTime;
    public float StartTime => startTime;
    [SerializeField] private int minSpawnCount = 1;
    public int MinSpawnCount => minSpawnCount;

    [SerializeField] private float spawnInterval = 1f;
    public float SpawnInterval => spawnInterval;

    [Tooltip("このフェーズで出る敵の最大HPに掛ける倍率")]
    [SerializeField, Min(0f)] private float hpMultiplier = 1f;
    public float HpMultiplier => hpMultiplier;

    [Tooltip("このフェーズで出る敵の攻撃力（弾・接触・爆発のダメージ）に掛ける倍率")]
    [SerializeField, Min(0f)] private float attackMultiplier = 1f;
    public float AttackMultiplier => attackMultiplier;

    [SerializeField] private List<EnemySpawnData> enemies;
    public IReadOnlyList<EnemySpawnData> Enemies => enemies;
}

[System.Serializable]
public class SpawnDirectionData
{
    [Range(0f, 180f)]
    public float angle;

    [Min(0f)]
    public float weight;
}