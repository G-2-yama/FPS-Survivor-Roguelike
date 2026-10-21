using UnityEngine;

public class KnockbackState : IState
{
    private readonly EnemyBrain enemy;
    private readonly StateMachine<IState> stateMachine;

    private Vector3 knockbackVelocity;
    private float timer;
    private float nextKnockbackTime = float.NegativeInfinity;

    public KnockbackState(EnemyBrain enemy, StateMachine<IState> stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    /// <summary>硬直中と終了後の耐性中は追加のノックバックを受け付けない</summary>
    public bool TryEnter(Vector3 direction, float force)
    {
        float finalForce = force * (1f - enemy.Config.KnockbackResistance);
        if (stateMachine.CurrentState == this || Time.time < nextKnockbackTime || finalForce <= 0f)
            return false;

        knockbackVelocity = direction.normalized * finalForce;
        stateMachine.ChangeState(this);
        return true;
    }

    /// <summary>プールから再出現した個体の耐性時間を初期化する</summary>
    public void ResetCooldown()
    {
        nextKnockbackTime = float.NegativeInfinity;
    }

    public void Enter()
    {
        timer = enemy.Config.KnockbackDuration;
    }

    public void Update()
    {
        timer -= Time.fixedDeltaTime;

        enemy.Rb.MovePosition(enemy.Rb.position + knockbackVelocity * Time.fixedDeltaTime);

        knockbackVelocity *= 0.9f;

        if (timer <= 0f)
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }

    public void Exit()
    {
        nextKnockbackTime = Time.time + enemy.Config.KnockbackCooldown;
    }
}
