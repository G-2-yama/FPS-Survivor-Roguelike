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

    [SerializeField] private List<EnemySpawnData> enemies;
    public IReadOnlyList<EnemySpawnData> Enemies => enemies;

    [Header("Ex Phase HP Scaling")]
    [SerializeField, Tooltip("直前の通常PhaseのDPSに応じて、生成する個体のHPだけを補正する")]
    private bool isExPhase;
    [SerializeField, Min(0.01f), Tooltip("このDPS以下は基礎HPのまま。超えた分はDPS比の平方根で緩やかに上乗せする")]
    private float referenceDps = 100f;
    [SerializeField, Min(1f), Tooltip("HP倍率の下限。基礎HP未満にはならない")]
    private float minHpMultiplier = 1f;
    [SerializeField, Min(1f), Tooltip("HP倍率の上限。標準は1.3倍")]
    private float maxHpMultiplier = 1.3f;

    public bool IsExPhase => isExPhase;
    public float ReferenceDps => Mathf.Max(0.01f, referenceDps);
    public float MinHpMultiplier => Mathf.Max(1f, minHpMultiplier);
    public float MaxHpMultiplier => Mathf.Max(MinHpMultiplier, maxHpMultiplier);
}

[System.Serializable]
public class SpawnDirectionData
{
    [Range(0f, 180f)]
    public float angle;

    [Min(0f)]
    public float weight;
}
