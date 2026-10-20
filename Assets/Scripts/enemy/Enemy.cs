using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary>敵の体力、ダメージ反応、ドロップ、プール状態を管理する</summary>
[RequireComponent(typeof(DamageTargetBinding))]
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
    [SerializeField] private MonoBehaviour contactDamageSource;
    [SerializeField] private PlayerObstacle playerObstacle;
    [SerializeField] private Collider[] layerColliders;
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
    [SerializeField, Tooltip("敵Prefabに事前配置したプレイヤー接触判定の対象")]
    private PlayerContactTarget contactTarget;

    /// <summary>Editorで設定した必須の接触対象を検査する</summary>
    private void Awake()
    {
        if (contactTarget == null)
            throw new System.InvalidOperationException($"{name}: Contact TargetをEditorで設定してください");
    }

    /// <summary>敵の活動期間を管理する生成器を設定する</summary>
    /// <param name="generator">この敵を生成したジェネレーター</param>
    public void SetSpawner(EnemyGenerator generator)
    {
        this.generator = generator;
        enemyBrain.SetSpawner(generator);
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
        if (isDead)
            return;

        Target = enemyBrain.Target;
        if (Target != null)
        {
            Vector3 dir = transform.position - Target.position;

            enemyBrain.KnockbackState.TryEnter(dir, knockbackForce);
        }
    }

    /// <summary>プールから取得されたとき敵の状態を戻し、体力を再生成する</summary>
    public override void OnGet()
    {
        AssignEnemyLayerToColliders();
        damageEffectManager = DamageEffectManager.Instance;
        enemyBrain.ResetBrain();
        isDead = false;

        contactTarget.BeginLife(
            enemyBrain.Rb,
            contactDamageSource,
            playerObstacle == null);
        finalDamage = 0;
        Health = new Health(config.MaxHp);
        Health.OnDamaged += HandleDamaged;
        Health.OnDeath += HandleDeath;
    }

    /// <summary>通常の敵レイヤーを適用し、専用コンポーネントを持つ例外オブジェクトには専用レイヤーを再適用する</summary>
    private void AssignEnemyLayerToColliders()
    {
        if (playerObstacle != null)
        {
            playerObstacle.ApplyLayerToColliders();
            return;
        }

        const string layerName = "Enemy";
        int collisionLayer = LayerMask.NameToLayer(layerName);
        if (collisionLayer < 0)
        {
            Debug.LogError($"{layerName}レイヤーが登録されていません", this);
            return;
        }

        if (layerColliders == null) return;
        foreach (Collider enemyCollider in layerColliders)
            if (enemyCollider != null) enemyCollider.gameObject.layer = collisionLayer;
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
        if(DestroyDamagePrefab != null)
        {
            PooledInstance explosion = PoolManager.Instance.Rent(DestroyDamagePrefab);
            GameObject destroyDamage = explosion.GameObject;
            destroyDamage.transform.position = transform.position;
            destroyDamage.transform.rotation = transform.rotation;
            if ((int)(finalDamage * destroyDamageDamping) > 0)
            {
                explosion.RequireDamage().Initialize((int)(finalDamage * destroyDamageDamping), 0);
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


