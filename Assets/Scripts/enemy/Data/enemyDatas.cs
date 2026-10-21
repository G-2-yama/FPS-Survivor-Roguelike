using UnityEngine;

/// <summary>敵の能力とプレイヤー接触時の挙動を設定する</summary>
[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Status")]
    [Min(1)] public int MaxHp = 5;
    [SerializeField] private int damagelange = 1;
    [SerializeField][Range(0f, 1f)] private float knockbackResistance = 0f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField, Min(0f), Tooltip("ノックバック終了後、再びノックバックを受けるまでの秒数。ダメージは通常どおり受ける")]
    private float knockbackCooldown = 1f;
    public int Damagelange => damagelange;
    public float KnockbackResistance => knockbackResistance;
    public float KnockbackDuration => knockbackDuration;
    public float KnockbackCooldown => Mathf.Max(0f, knockbackCooldown);





    [Header("Movement")]
    [Min(0f)] [SerializeField] private float engagedistance = 5f;
    public float EngageDistance => engagedistance;
    public MovementPattern chaseMovedata;
    public MovementPattern combatMovedata;

   


    [Header("Combat")]
    [Min(0)] public int AttackPower = 1;
    [Min(0.01f)] public float AttackInterval = 3f;
    public AttackPattern AttackPattern;

    [Header("Minimap")]
    [SerializeField, Tooltip("有効にするとミニマップにEx敵用の色で表示する")]
    private bool isExEnemy;
    public bool IsExEnemy => isExEnemy;


    


}
