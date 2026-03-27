using UnityEngine;

/// <summary>
/// 重力とジャンプを含む縦方向速度を管理するクラス
/// </summary>
public class PlayerVerticalMover
{
    private readonly PlayerConfig config;
    private float verticalVelocity;

    public PlayerVerticalMover(PlayerConfig config)
    {
        this.config = config;
    }

    public float VerticalVelocity => verticalVelocity;

    public void UpdateVerticalVelocity(bool isGrounded)
    {
        if (isGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = config.GroundedVerticalVelocity;
            return;
        }

        verticalVelocity += config.Gravity * Time.deltaTime;
    }

    public void Jump()
    {
        verticalVelocity = config.JumpSpeed;
    }

    public void ResetGroundedVelocity()
    {
        verticalVelocity = config.GroundedVerticalVelocity;
    }
}
