using UnityEngine;

/// <summary>
/// プレイヤー移動処理を統合し、各移動コンポーネントを仲介するクラス
/// </summary>
public class PlayerMover
{
    private readonly Transform playerTransform;
    private readonly PlayerGroundProbe groundProbe;
    private readonly PlayerHorizontalMover horizontalMover;
    private readonly PlayerVerticalMover verticalMover;

    /// <summary>
    /// PlayerMoverを初期化する
    /// </summary>
    /// <param name="playerTransform">移動対象となるプレイヤーのTransform</param>
    /// <param name="config">移動・重力に関する設定</param>
    public PlayerMover(Transform playerTransform, PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        groundProbe = new PlayerGroundProbe(playerTransform, config);
        horizontalMover = new PlayerHorizontalMover(playerTransform, config);
        verticalMover = new PlayerVerticalMover(config);
    }

    /// <summary>
    /// プレイヤーの移動を処理する
    /// </summary>
    /// <param name="moveInput">移動入力</param>
    /// <param name="isRunning">走行状態</param>
    public void Move(Vector2 moveInput, bool isRunning)
    {
        bool isGrounded = groundProbe.IsGrounded();
        verticalMover.UpdateVerticalVelocity(isGrounded);

        Vector3 groundNormal = Vector3.up;
        if (isGrounded && groundProbe.TryGetGroundNormal(out Vector3 hitNormal))
        {
            groundNormal = hitNormal;
        }

        Vector3 frameMove = horizontalMover.CalculateHorizontalVelocity(moveInput, isRunning, groundNormal);
        frameMove.y = verticalMover.VerticalVelocity;

        Vector3 nextPosition = playerTransform.position + frameMove * Time.deltaTime;

        if (groundProbe.TryGetGroundHit(nextPosition, out RaycastHit groundHit) && verticalMover.VerticalVelocity <= 0f)
        {
            nextPosition.y = groundHit.point.y;
            verticalMover.ResetGroundedVelocity();
        }

        playerTransform.position = nextPosition;
    }

    /// <summary>
    /// 現在フレームで接地しているかを判定する
    /// </summary>
    /// <returns>接地していればtrue、空中であればfalse</returns>
    public bool IsGrounded()
    {
        return groundProbe.IsGrounded();
    }

    /// <summary>
    /// ジャンプを開始する
    /// </summary>
    public void Jump()
    {
        verticalMover.Jump();
    }
}
