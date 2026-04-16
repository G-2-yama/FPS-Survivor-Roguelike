using UnityEngine;

public class CircleState : IState
{
    private moveenemy enemy;
    private StateMachine<IState> stateMachine;
    private float shotTimer;

    public CircleState(moveenemy enemy, StateMachine<IState> sm)
    {
        this.enemy = enemy;
        this.stateMachine = sm;
    }

    public void Enter() => shotTimer = 0;

    public void Update()
    {
        if (enemy.Target == null) return;

        Vector3 toTarget = enemy.Target.position - enemy.transform.position;
        if (toTarget.magnitude > enemy.Length)
        {
            stateMachine.ChangeState(new ChaseState(enemy, stateMachine));
            return;
        }

        // --- 円移動ロジック ---
        Vector3 offset = enemy.transform.position - enemy.Target.position;
        offset = offset.normalized * enemy.Radius;
        offset = Quaternion.AngleAxis(enemy.EnemyDatas.Speed * 3 * Time.deltaTime, Vector3.up) * offset;

        Vector3 nextPos = enemy.Target.position + offset;
        nextPos.y = Mathf.Lerp(enemy.transform.position.y, enemy.Target.position.y, Time.deltaTime * 2f);

        enemy.transform.position = nextPos;
        enemy.transform.rotation = Quaternion.LookRotation(-1*(enemy.Target.position - enemy.transform.position));

        // --- 攻撃ロジック ---
        shotTimer += Time.deltaTime;
        if (shotTimer >= enemy.EnemyDatas.ShotInterval)
        {
            enemy.EnemyDatas.AttackLogic?.Execute(enemy, enemy.ShotPoint);
            shotTimer = 0;
        }
    }

    public void Exit()
    {
        // 状態を抜けるときに攻撃をキャンセル（必要に応じて）
        enemy.EnemyDatas.AttackLogic?.Cancel(enemy);
    }
}