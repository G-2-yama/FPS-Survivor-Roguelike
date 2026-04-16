using UnityEngine;

public class Enemycondition : PoolableObject, IDamageable
{
    [SerializeField] private enemyDatas enemyData;
    [SerializeField] private moveenemy moveEnemy; // 本体への参照

    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }

    public void Init()
    {
        Health = new Health(enemyData.Hp);
        Health.OnDeath += HandleDeath;
    }

    void HandleDeath() => Release();

    public void TakeDamage(int damage) => Health.TakeDamage(damage);

    public override void OnGet()
    {
        Init();
        // 必要に応じてここで初期ステートをセットし直すなどの処理
    }

    public override void OnRelease()
    {
        // 重要：プールに戻る際、進行中の攻撃を完全に止める
        moveEnemy.EnemyDatas.AttackLogic?.Cancel(moveEnemy);

        if (Health != null)
            Health.OnDeath -= HandleDeath;
    }
}