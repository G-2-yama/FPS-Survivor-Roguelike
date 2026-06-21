using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "PhaseData", menuName = "EnemyGenerator/PhaseData")]
public class PhaseData : ScriptableObject
{
    [SerializeField] private float startTime;
    public float StartTime => startTime;

    [SerializeField] private float spawnInterval = 1f;
    public float SpawnInterval => spawnInterval;

    [SerializeField] private List<EnemySpawnData> enemies;
    public IReadOnlyList<EnemySpawnData> Enemies => enemies;
}