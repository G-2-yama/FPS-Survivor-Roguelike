using UnityEngine;

public class CombatState : IState
{
    private readonly EnemyBrain enemy;
    private readonly StateMachine<IState> stateMachine;

    public CombatState(
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
           
            enemy.Animator.SetBool("Iscombat", true);
        }
    }

    public void Update()
    {
        if (enemy.Target == null)
            return;

        float distance = Vector3.Distance(
            enemy.transform.position,
            enemy.Target.position);

        if (distance >
            enemy.Config.EngageDistance + 1f)
        {
            stateMachine.ChangeState(
                enemy.ChaseState);

            return;
        }

        enemy.Config.combatMovedata.Move(
            enemy.Rb,
            enemy.transform,
            enemy.Target);

        enemy.Attack?.TryAttack();
    }

    public void Exit()
    {
        enemy.Attack?.CancelAttack();
    }
}
