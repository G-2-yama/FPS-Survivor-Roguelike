using UnityEngine;
using System;
using System.Collections;

public class Enemycondition : MonoBehaviour, IDamageable
{
    [SerializeField] private enemyDatas enemydata;
    [SerializeField] private enemybulletData enemybulletdata;
    [SerializeField] private Transform shotpoint;

    private Transform player;
    private Vector3 playerposition;

    public TeamType TeamType => TeamType.Enemy;

    public Health Health { get; private set; }

    Action onDeathReturn;

    Coroutine shotCoroutine;

    // 初期化（プールから取り出されたときに呼ばれる）
    public void Init(Action returnAction)
    {
        onDeathReturn = returnAction;

        // HPリセット
        Health = new Health(enemydata.Hp);
        Health.OnDeath += HandleDeath;
    }

    private void HandleDeath()
    {
        onDeathReturn?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    void OnEnable()
    {
        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }

        shotCoroutine = StartCoroutine(shot());
    }

    void OnDisable()
    {
        if (shotCoroutine != null)
        {
            StopCoroutine(shotCoroutine);
        }
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