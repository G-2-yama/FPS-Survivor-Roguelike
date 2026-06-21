using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [SerializeField] private int spawnWeight = 1;
    public int SpawnWeight => spawnWeight;
}