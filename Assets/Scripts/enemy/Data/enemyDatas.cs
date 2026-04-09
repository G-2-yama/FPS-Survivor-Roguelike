using UnityEngine;
[CreateAssetMenu(menuName = "enemy/Data")]

public class enemyDatas : ScriptableObject
{
    [SerializeField] private int hp = 5;
    public int Hp => hp;
    [SerializeField] private float speed = 8f;
    public float Speed => speed;
    [SerializeField] int atk = 1;
    public int Atk => atk;

}
