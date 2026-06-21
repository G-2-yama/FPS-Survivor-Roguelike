using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Status")]
    [Min(1)] public int MaxHp = 5;
    [SerializeField] private int damagelange = 1;
    public int Damagelange => damagelange;


    [Header("Movement")]
    [Min(0f)] public float EngageDistance = 5f;
    public MovementPattern chaseMovedata;
    public MovementPattern combatMovedata;


    [Header("Combat")]
    [Min(0)] public int AttackPower = 1;
    [Min(0.01f)] public float AttackInterval = 3f;
    public AttackPattern AttackPattern;
    
}
