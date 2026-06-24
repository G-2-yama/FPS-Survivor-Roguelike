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
    [Min(0f)] private float engagedistance = 5f;
    public float EngageDuration => engagedistance;
    public MovementPattern chaseMovedata;
    public MovementPattern combatMovedata;

    public void IncreaseDistance(float amount)
    {
        engagedistance += amount;
    }
    public void DecreaseDistance(float amount)
    {
        if (engagedistance < 0)
        {
            Debug.Log("engagedistance < 0");
            return;
        }
        engagedistance -= amount;
    }



    [Header("Combat")]
    [Min(0)] public int AttackPower = 1;
    [Min(0.01f)] public float AttackInterval = 3f;
    public AttackPattern AttackPattern;

    
}
