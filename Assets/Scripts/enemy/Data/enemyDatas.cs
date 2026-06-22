using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Status")]
    [Min(1)] public int MaxHp = 5;
    [SerializeField] private int damagelange = 1;
    [SerializeField][Range(0f, 1f)] private float knockbackResistance = 0f;
    [SerializeField] private float knockbackDuration = 0.2f;
    public int Damagelange => damagelange;
    public float KnockbackResistance => knockbackResistance;
    public float KnockbackDuration => knockbackDuration;


    [Header("Movement")]
    [Min(0f)] public float EngageDistance = 5f;
    public MovementPattern chaseMovedata;
    public MovementPattern combatMovedata;


    [Header("Combat")]
    [Min(0)] public int AttackPower = 1;
    [Min(0.01f)] public float AttackInterval = 3f;
    public AttackPattern AttackPattern;
    
}
