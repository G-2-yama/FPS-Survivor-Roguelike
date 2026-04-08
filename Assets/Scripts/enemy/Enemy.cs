using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class Enemycondition : MonoBehaviour, IDamageable
{
    [SerializeField] private enemyDatas enemydata;
    [SerializeField] private enemybulletData enemybulletdata;
    [SerializeField] private Transform shotpoint;

    private Vector3 playerposition;
    public TeamType TeamType => TeamType.Enemy;

    /// <summary>
    /// 体力を管理するモデル
    /// </summary>
    public Health Health { get; private set; }

    private void Awake()
    {
        Health = new Health(enemydata.Hp);
        Health.OnDeath += HandleDeath;
       
    }
    
    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    /// <summary>
    /// 死亡したときに呼び出される処理
    /// </summary>
    private void HandleDeath()
    {
        Destroy(this.gameObject);
    }

    private Transform player;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        StartCoroutine(shot());
    }

    void Update()
    {
        if (player == null) return;
        playerposition = player.position - transform.position;
    }

    IEnumerator shot()
    {
        yield return null;

        while (true)
        {
            if (playerposition.sqrMagnitude > 0.001f)
            {
                Vector3 dir = playerposition.normalized;
                enemybulletdata.Shot(shotpoint, dir);
            }

            yield return new WaitForSeconds(3f);
        }
    }
}

