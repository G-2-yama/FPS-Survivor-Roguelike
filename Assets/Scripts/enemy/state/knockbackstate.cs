using UnityEngine;

public class KnockbackState : IState
{
    private readonly EnemyBrain enemy;
    private readonly StateMachine<IState> stateMachine;

    private Vector3 knockbackVelocity;
    private float duration;
    private float timer;

    public KnockbackState(
        EnemyBrain enemy,
        StateMachine<IState> stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public void SetKnockback(
        Vector3 direction,
        float force,
        float duration)
    {
        knockbackVelocity =
            direction.normalized * force;

        this.duration = duration;
    }

    public void Enter()
    {
        timer = duration;
    }

    public void Update()
    {
        timer -= Time.fixedDeltaTime;

        enemy.Rb.MovePosition(
            enemy.Rb.position +
            knockbackVelocity * Time.fixedDeltaTime);

        knockbackVelocity *= 0.9f;

        if (timer <= 0f)
        {
            stateMachine.ChangeState(
                enemy.ChaseState);
        }
    }

    public void Exit()
    {
    }
}