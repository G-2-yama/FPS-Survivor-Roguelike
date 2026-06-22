using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DamageArea : PoolableObject, IDamageable
{
    [SerializeField] private Collider areaCollider;
    [SerializeField] private TeamType myTeam;
    [SerializeField] private TeamType targetTeam;
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float interval = 1f;
    [SerializeField] private float lifetime = 0f;
    [SerializeField] private DamageAreaMode mode;

    [SerializeField] private int HP = 1;
    private Health health;
    public TeamType TeamType => myTeam;

    public void TakeDamage(int damage, float knockbackForce)
    {
        health.TakeDamage(damage);
        if (health.IsDead)
        {
            Release();
        }
    }


    /// <summary>
    /// 各対象ごとのダメージタイマーを管理する辞書
    /// </summary>
    private Dictionary<IDamageable, float> timers = new Dictionary<IDamageable, float>();
    private Coroutine lifeRoutine;

    /// <summary>
    /// ２重リリース防止のフラグ
    /// </summary>
    private bool isReleased;

    /// <summary>
    /// DamageAreaを有効化
    /// </summary>
    public override void OnGet()
    {
        isReleased = false;
        health = new Health(HP);
        if (lifetime > 0f)
            lifeRoutine = StartCoroutine(LifeTimer(lifetime));
    }

    public void Initialize(int damage, float knockbackForce)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        OnGet();
    }

    /// <summary>
    /// DamageAreaを無効化
    /// </summary>
    public override void OnRelease()
    {
        isReleased = true;
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }
        timers.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mode != DamageAreaMode.OnEnterOnce)
            return;

        var damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            return;

        if ((damageable.TeamType & targetTeam) == 0)
            return;

        damageable.TakeDamage(damage, knockbackForce);
    }

    private void OnTriggerStay(Collider other)
    {
        if (mode != DamageAreaMode.Interval)
            return;

        var damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            return;

        if ((damageable.TeamType & targetTeam) == 0)
            return;

        if (!timers.ContainsKey(damageable))
            timers[damageable] = 0f;

        timers[damageable] += Time.deltaTime;

        if (timers[damageable] >= interval)
        {
            damageable.TakeDamage(damage, knockbackForce);
            timers[damageable] = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isReleased) return;

        var damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        timers.Remove(damageable);
    }

    private IEnumerator LifeTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if (!isReleased) Release();
    }
}

public enum DamageAreaMode
{
    OnEnterOnce, // 入った瞬間に一度だけダメージ
    Interval,      // 入っている間、一定間隔でダメージ
}