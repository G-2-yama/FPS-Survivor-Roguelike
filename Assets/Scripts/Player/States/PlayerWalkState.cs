using UnityEngine;

public class PlayerWalkState : PlayerMoveState
{
    public PlayerWalkState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Walk Stateに入りました");
    }

    /// <summary>
    /// 歩行状態の更新を行い、入力がない場合は待機状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (model.moveInput == Vector2.zero)
        {
            controller.StateMachine.ChangeState(new PlayerIdleState(controller));
            return;
        }

        Move();
    }

    /// <summary>
    /// 入力方向と速度に基づいてプレイヤーを移動
    /// </summary>
    private void Move()
    {
        Transform movementBasis = model.transform;

        Vector3 move =
            movementBasis.right * model.moveInput.x +
            movementBasis.forward * model.moveInput.y;

        move.y = 0f;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        float speed = model.isRunning ? model.RunSpeed : model.WalkSpeed;
        controller.transform.position += move * speed * Time.deltaTime;
    }
}
