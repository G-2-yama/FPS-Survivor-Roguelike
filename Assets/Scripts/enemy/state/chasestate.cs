using UnityEngine;

public class ChaseState : IState
{
    private readonly EnemyBrain enemy;
    private readonly StateMachine<IState> stateMachine;

    public ChaseState(
        EnemyBrain enemy,
        StateMachine<IState> stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        if (enemy.Animator != null)
        {
           
            enemy.Animator.SetBool("Iscombat", false);
            
        }
    }

    public void Update()
    {
        if (enemy.Target == null)
            return;

        float distance = Vector3.Distance(
            enemy.transform.position,
            enemy.Target.position);

        if (distance <= enemy.Config.EngageDistance)
        {
            stateMachine.ChangeState(
                enemy.CombatState);

            return;
        }

        enemy.Config.chaseMovedata.Move(
            enemy.Rb,
            enemy.transform,
            enemy.Target);
    }
    public void Exit()
    {
    }
}
