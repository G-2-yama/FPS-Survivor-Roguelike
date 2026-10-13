using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary>敵の体力、ダメージ反応、ドロップ、プール状態を管理する</summary>
public class Enemy : PoolableObject, IDamageable
{
    /// <summary>敵のドロップ候補と選択時の重みを定義する</summary>
    [System.Serializable]
    public class DropData
    {
        public GameObject prefab;
        [Min(1)]
        public int weight = 1;
    }
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyAttackController attackController;
    private GameObject item;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField] private EnemyBrain enemyBrain;
    Transform Target;


    [SerializeField] private float deathDelay = 0.1f;
    [SerializeField] private float destroyDamageDamping = 0.7f;

    [SerializeField] private List<DropData> dropList = new();
    [SerializeField] private Sounder sounder;
    [SerializeField] private GameObject DestroyDamagePrefab;
    public Sounder Sounder => sounder;
    private DamageEffectManager damageEffectManager;

    public static event System.Action<int> OnAnyDamaged;
    public static event System.Action OnAnyKilled;

    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }

    private bool isDead = false;
    private EnemyGenerator generator;
    private int finalDamage = 0;
    private PlayerContactTarget contactTarget;


    /// <summary>敵の活動期間を管理する生成器を設定する</summary>
    /// <param name="generator">この敵を生成したジェネレーター</param>
    public void SetSpawner(EnemyGenerator generator)
    {
        this.generator = generator;
    }


    /// <summary>設定されたダメージを適用し、可能な場合はノックバック状態へ移行する</summary>
    /// <param name="damage">受ける基礎ダメージ</param>
    /// <param name="knockbackForce">敵の状態へ渡すノックバック量</param>
    public void TakeDamage(int damage, float knockbackForce)
    {


        // ダメージ処理
        if (Health == null || isDead)
            return;

        sounder?.Play(SoundCategory.Damage);

        if (Health.CurrentHP > 0)
        {
            whiteFlash?.Flash();
        }
        damageEffectManager?.ShowDamage(damage, transform.position);


        Health.TakeDamage(config.Damagelange * damage);
        finalDamage = config.Damagelange * damage;

        // ノックバック処理
        Target = enemyBrain.Target;
        if (Target != null)
        {
            Vector3 dir = transform.position - Target.position;

            enemyBrain.KnockbackState.SetKnockback(dir, knockbackForce);

            enemyBrain.ChangeState(enemyBrain.KnockbackState);
        }
    }

    /// <summary>プールから取得されたとき敵の状態を戻し、体力を再生成する</summary>
    public override void OnGet()
    {
        AssignEnemyLayerToColliders();
        damageEffectManager = DamageEffectManager.Instance;
        enemyBrain.ResetBrain();
        isDead = false;
        if (contactTarget == null)
        {
            contactTarget = GetComponent<PlayerContactTarget>();
            if (contactTarget == null) contactTarget = gameObject.AddComponent<PlayerContactTarget>();
        }
        contactTarget.BeginLife(enemyBrain.Rb, GetComponentInChildren<ToutchDamageController>());

        Health = new Health(config.MaxHp);
        Health.OnDamaged += HandleDamaged;
        Health.OnDeath += HandleDeath;
    }

    /// <summary>敵のすべてのColliderをプレイヤーとの物理判定用レイヤーに設定する</summary>
    private void AssignEnemyLayerToColliders()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0)
            return;

        foreach (Collider enemyCollider in GetComponentsInChildren<Collider>(true))
            enemyCollider.gameObject.layer = enemyLayer;
    }

    /// <summary>プールへ戻す前に攻撃を停止し、体力イベントの購読を解除する</summary>
    public override void OnRelease()
    {
        contactTarget?.Invalidate();
        attackController?.CancelAttack();

        if (Health != null)
        {
            Health.OnDamaged -= HandleDamaged;
            Health.OnDeath -= HandleDeath;
            Health = null;
        }
    }

    /// <summary>敵に適用されたダメージ量を通知する</summary>
    /// <param name="appliedDamage">体力システムが適用したダメージ</param>
    /// <param name="currentHP">現在の敵の体力</param>
    /// <param name="maxHP">敵の最大体力</param>
    private void HandleDamaged(int appliedDamage, int currentHP, int maxHP)
    {
        OnAnyDamaged?.Invoke(appliedDamage);
    }

    /// <summary>敵の体力が0になった際に一度だけ死亡処理を開始する</summary>
    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        contactTarget?.Invalidate();
        OnAnyKilled?.Invoke();

        StartCoroutine(DeathRoutine());
    }

    /// <summary>死亡までの時間を待ち、報酬を生成して敵をプールへ戻す</summary>
    private IEnumerator DeathRoutine()
    {

        yield return new WaitForSeconds(deathDelay);

        GameObject prefab = GetRandomDrop();

        if (prefab != null)
        {
            GameObject item = PoolManager.Instance.Get(prefab);
            item.transform.position = transform.position;
            item.transform.rotation = transform.rotation;
        }
        if(DestroyDamagePrefab != null)
        {
            GameObject destroyDamage = PoolManager.Instance.Get(DestroyDamagePrefab);
            destroyDamage.transform.position = transform.position;
            destroyDamage.transform.rotation = transform.rotation;
            if ((int)(finalDamage * destroyDamageDamping) > 0)
            {
                destroyDamage.GetComponent<DamageBase>().Initialize((int)(finalDamage * destroyDamageDamping), 0);
            }
        }
        generator.RemoveActiveEnemy(this.gameObject);
        Release();
    }

     /// <summary>設定された重み付きドロップ一覧からPrefabを選択する</summary>
     /// <returns>選択したPrefab、候補がない場合はnull</returns>
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


