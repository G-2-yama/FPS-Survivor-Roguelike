using UnityEngine;

[CreateAssetMenu(menuName = "enemy/Data")]
public class enemyDatas : ScriptableObject
{
    public int Hp = 5;
    public float Speed = 8f;
    public int Atk = 1;
    public float ShotInterval = 3f;
    public EnemyAttackBase AttackLogic; // ‚±‚±‚ÉBurstShotLogic‚È‚Ç‚ð“ü‚ê‚é
}