using UnityEngine;

public class CircleState : IState
{
    private readonly EnemyBrain enemy;
    private readonly StateMachine<IState> stateMachine;

    public CircleState(EnemyBrain enemy, StateMachine<IState> stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public void Enter() { }

    public void Update()
    {
        if (enemy.Target == null)
            return;

        float distance = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        if (distance > enemy.Config.EngageDistance)
        {
            stateMachine.ChangeState(new ChaseState(enemy, stateMachine));
            return;
        }

        enemy.Movement.OrbitAround(
            enemy.transform,
            enemy.Target,
            enemy.Config.OrbitRadius,
            enemy.Config.OrbitAngularSpeed);

        enemy.Attack.TryAttack();
    }

    public void Exit()
    {
        enemy.Attack.CancelAttack();
    }
}
