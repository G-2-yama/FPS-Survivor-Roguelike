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
        if (enemy.PoseSwitcher != null)
        {
            enemy.PoseSwitcher.SetCombat(false);
        }
        if(enemy.UniqueAnimation != null)
        {
            enemy.UniqueAnimation.Stop();
        }
    }

    public void Update()
    {
        Transform target = enemy.Target;
        if (target == null)
            return;

        Vector3 targetPosition = EnemyBrainRunner.GetTargetPosition(target);
        float distance = Vector3.Distance(
            enemy.CachedTransform.position,
            targetPosition);

        if (distance <= enemy.EngageDistance)
        {
            stateMachine.ChangeState(
                enemy.CombatState);

            return;
        }

        if (enemy.TryRelocate(distance, targetPosition))
            return;

        enemy.UpdateFar(distance);

        enemy.Config.chaseMovedata.Move(
            enemy.Rb,
            enemy.CachedTransform,
            target,
            targetPosition,
            enemy.TickSteps);
    }
    public void Exit()
    {
        enemy.ClearFar();
    }
}
