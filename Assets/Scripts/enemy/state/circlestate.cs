using UnityEngine;

public class CircleState : IState
{
    private moveenemy enemy;
    private StateMachine<IState> stateMachine;

    public CircleState(moveenemy enemy, StateMachine<IState> sm)
    {
        this.enemy = enemy;
        this.stateMachine = sm;
    }

    public void Enter() { }

    public void Update()
    {
        Vector3 toTarget = enemy.Target.position - enemy.transform.position;
        float distance = toTarget.magnitude;

        if (distance > enemy.Length)
        {
            stateMachine.ChangeState(new ChaseState(enemy, stateMachine));
            return;
        }

        Vector3 offset = enemy.transform.position - enemy.Target.position;
       
        offset = offset.normalized * enemy.Radius;

        enemy.transform.rotation = Quaternion.LookRotation(offset);

        offset = Quaternion.AngleAxis(enemy.EnemyDatas.Speed *3* Time.deltaTime, enemy.Target.up) * offset;
        enemy.transform.position = enemy.Target.position + offset;
    }


    public void Exit() { }
}