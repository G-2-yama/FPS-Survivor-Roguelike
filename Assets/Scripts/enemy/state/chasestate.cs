using UnityEngine;

public class ChaseState : IState
{
    private moveenemy enemy;
    private StateMachine<IState> stateMachine;

    public ChaseState(moveenemy enemy, StateMachine<IState> sm)
    {
        this.enemy = enemy;
        this.stateMachine = sm;
    }

    public void Enter() { }

    public void Update()
    {
        Vector3 toTarget = enemy.Target.position - enemy.transform.position;
        float distance = toTarget.magnitude;

        if (distance <= enemy.Length)
        {
            stateMachine.ChangeState(new CircleState(enemy, stateMachine));
            return;
        }

        enemy.transform.position += toTarget.normalized * enemy.EnemyDatas.Speed / 3 * Time.deltaTime;
        enemy.transform.rotation = Quaternion.LookRotation(-toTarget);
    }

    public void Exit() { }
}
