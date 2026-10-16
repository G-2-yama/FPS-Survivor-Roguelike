using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>敵の攻撃個体の体力と命中とドロップを管理する</summary>



[RequireComponent(typeof(DamageTargetBinding))]
public class enemyplojectileobject : PoolableObject, IDamageable
{
    /// <summary>ドロップするPrefabと選択時の重みを定義する</summary>
    [System.Serializable]
    public class DropData
    {
        public GameObject prefab;
        [Min(1)]
        public int weight = 1;
    }
    private DeathType deathType = DeathType.Normal;
    [SerializeField] private List<DropData> dropList = new();
    [SerializeField] protected EnemyConfig config;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField] private float deathDelay = 0.1f;
    [SerializeField] private EnemyBrain enemyBrain;
    [SerializeField] private Sounder sounder;
    private DamageEffectManager damageEffectManager;
    protected Health health;
    public Sounder Sounder => sounder;
    public TeamType TeamType => TeamType.EnemyAmmo;
    public TeamType targetTeam => TeamType.Player;

    protected bool hasHit = false;
    protected int damage;
    protected float knockbackForce;
    protected float attackMultiplier = 1f;
    private EnemyGenerator generator;
    /// <summary>個体を生成した敵生成器を関連付ける</summary>

    public void SetSpawner(EnemyGenerator generator)
    {
        this.generator = generator;
        if (enemyBrain != null)
        {
            enemyBrain.SetSpawner(generator);
        }
    }
    /// <summary>取得した個体へ今回の攻撃設定を渡す</summary>

    public void Initialize(int damage, float knockbackForce)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;

    }

    public void ApplyStatMultipliers(float hpMultiplier, float attackMultiplier)
    {
        ReleaseHealth();
        CreateHealth(Mathf.Max(1, EnemyStatScaling.Scale(config.MaxHp, hpMultiplier)));
        this.attackMultiplier = attackMultiplier;
    }

    private void CreateHealth(int maxHp)
    {
        health = new Health(maxHp);
        health.OnDeath += HandleDeath;
    }

    private void ReleaseHealth()
    {
        if (health == null)
            return;

        health.OnDeath -= HandleDeath;
        health = null;
    }
    /// <summary>受けたダメージを個体の体力へ適用する</summary>


    public void TakeDamage(int damage, float knockbackForce)
    {
        if (health == null)
            return;
        if (deathType != DeathType.SelfDestruct)
        {
            sounder?.Play(SoundCategory.Damage);
        }
        damageEffectManager?.ShowDamage(damage,transform.position);

        if (health.CurrentHP > 0)
        {
            whiteFlash?.Flash();
        }
       
        health?.TakeDamage(config.Damagelange*damage);
    }
    /// <summary>侵入したColliderとの接触処理を開始する</summary>



    protected void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        HandleHit(other);
    }
    /// <summary>新しい活動期間の状態を準備する</summary>

    public override void OnGet()
    {
        damageEffectManager=DamageEffectManager.Instance;
        hasHit = false;
        deathType= DeathType.Normal;
        CreateHealth(config.MaxHp);
        attackMultiplier = 1f;
        if (enemyBrain != null)
        {
            enemyBrain.ResetBrain();
        }

    }
    /// <summary>返却する個体の一時状態と購読を解除する</summary>
    
    public override void OnRelease()
    {
        hasHit = true;
        ReleaseHealth();

        if (generator != null)
        {
            generator.RemoveActiveEnemy(gameObject);
            generator = null;
        }
    }
    /// <summary>死亡した個体の通知と返却処理を開始する</summary>

    private void HandleDeath()
    {
        if (deathType == DeathType.Normal)
        {
            StartCoroutine(DeathRoutine());
        }
        else
        {
            Release();
        }
    }
    /// <summary>保持済みの登録情報を使って対象への命中を処理する</summary>

    protected virtual void HandleHit(Collider collider)
    {
        hasHit = true;
        deathType = DeathType.SelfDestruct;
        health?.TakeDamage(9999);
    }
    /// <summary>死亡後の待機とドロップ生成を処理する</summary>


    private IEnumerator DeathRoutine()
    {

        yield return new WaitForSeconds(deathDelay);

        GameObject prefab = GetRandomDrop();
        if (prefab != null)
        {
            PooledInstance spawnedItem = PoolManager.Instance.Rent(prefab);
            GameObject item = spawnedItem.GameObject;
            PickupTriggerItem pickup = spawnedItem.Pickup;
            if (pickup != null)
            {
                pickup.PlaceOnGround(transform.position, transform);
            }
            else
            {
                item.transform.position = transform.position;
                item.transform.rotation = transform.rotation;
            }
        }
        Release();
    }
    /// <summary>設定された重みからドロップするPrefabを選ぶ</summary>

    private GameObject GetRandomDrop()
    {
        if (dropList.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (var drop in dropList)
            totalWeight += drop.weight;

        int rand = Random.Range(0, totalWeight);

        foreach (var drop in dropList)
        {
            if (rand < drop.weight)
                return drop.prefab;

            rand -= drop.weight;
        }

        return null;
    }
}



