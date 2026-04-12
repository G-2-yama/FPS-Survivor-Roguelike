using UnityEngine;
using System.Collections;

public class Enemycondition : PoolableObject, IDamageable
{
    [SerializeField] private enemyDatas enemydata;
    [SerializeField] private enemybulletData enemybulletdata;
    [SerializeField] private Transform shotpoint;

    private Transform player;
    private Vector3 playerposition;

    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }

    Coroutine shotCoroutine;


    public void Init()
    {
        Health = new Health(enemydata.Hp);
        Health.OnDeath += HandleDeath;
    }

    void HandleDeath()
    {
        Release(); // ← 自分で帰る
    }

    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    public override void OnGet()
    {
        // プレイヤー取得（初回だけ）
        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }

        // HP初期化
        Init();

        // コルーチン開始
        shotCoroutine = StartCoroutine(Shot());
    }

    public override void OnRelease()
    {
        
        if (shotCoroutine != null)
        {
            StopCoroutine(shotCoroutine);
            shotCoroutine = null;
        }

      
        if (Health != null)
        {
            Health.OnDeath -= HandleDeath;
        }
    }

    void Update()
    {
        if (player == null) return;
        playerposition = player.position - transform.position;
    }

    IEnumerator Shot()
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