using UnityEngine;

/// <summary>
/// 入力から地面に沿った水平移動量を計算するクラス
/// </summary>
public class PlayerHorizontalMover
{
    private readonly Transform playerTransform;
    private readonly PlayerConfig config;

    public PlayerHorizontalMover(Transform playerTransform, PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.config = config;
    }

    public Vector3 CalculateHorizontalVelocity(Vector2 moveInput, bool isRunning, Vector3 groundNormal)
    {
        Vector3 move =
            playerTransform.right * moveInput.x +
            playerTransform.forward * moveInput.y;

        float inputMagnitude = Mathf.Clamp01(move.magnitude);
        if (inputMagnitude <= 0f)
        {
            return Vector3.zero;
        }

        move.Normalize();
        move = Vector3.ProjectOnPlane(move, groundNormal).normalized;
        move *= inputMagnitude;

        float speed = isRunning ? config.RunSpeed : config.WalkSpeed;
        return move * speed;
    }
}
