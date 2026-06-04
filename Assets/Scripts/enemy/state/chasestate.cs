using UnityEngine;

public class ChaseState : IState
{
    private readonly EnemyBrain enemy;
    private readonly StateMachine<IState> stateMachine;

    public ChaseState(EnemyBrain enemy, StateMachine<IState> stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public void Enter() {
        if (enemy.animator != null)
        {
            enemy.animator.SetBool("Iscombat", false);
        }
    }

    public void Update()
    {
        if (enemy.Target == null)
            return;

        float distance = Vector3.Distance(
            enemy.transform.position,
            enemy.Target.position);

        if (distance <= enemy.Config.EngageDistance - 1.0f)
        {
            if (enemy.animator != null)
            {
                enemy.animator.SetBool("Iscombat", true);
            }
            stateMachine.ChangeState(
                new CombatState(enemy, stateMachine));
            return;
        }

        switch (enemy.ChaseMovementType)
        {
            case ChaseMovementType.Normal:

                enemy.Movement.MoveTowards(
                    enemy.Rb,
                    enemy.transform,
                    enemy.Target.position,
                    enemy.Config.ChaseSpeed); 

                break;

            case ChaseMovementType.Rolling:

                enemy.Movement.RollTowards(
                    enemy.Rb,
                    enemy.transform,
                    enemy.Target.position,
                    enemy.Config.ChaseSpeed);

                break;
        }
    }

    public void Exit() { 

    }
}
