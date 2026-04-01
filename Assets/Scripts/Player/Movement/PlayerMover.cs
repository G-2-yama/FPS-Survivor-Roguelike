using UnityEngine;

/// <summary>
/// Rigidbodyを使ったFPSプレイヤー移動処理
/// 設定値は PlayerConfig から取得する
/// </summary>
public class PlayerMover
{
    private Rigidbody rb;
    private Transform playerTransform;
    private PlayerConfig config;

    public PlayerMover(Transform playerTransform, Rigidbody rb,  PlayerConfig config)
    {
        this.rb = rb;
        this.playerTransform = playerTransform;
        this.config = config;

        rb.freezeRotation = true;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    public void Move(Vector2 moveInput, bool run)
    {
        Move(moveInput.x, moveInput.y, run);
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    public void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * config.JumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// 地面判定
    /// </summary>
    public bool IsGrounded()
    {
        Vector3 rayOrigin = playerTransform.position + Vector3.up * config.GroundRayStartOffset;

        bool isGrounded = Physics.Raycast(
            rayOrigin,
            Vector3.down,
            config.GroundCheckDistance,
            config.GroundLayers,
            QueryTriggerInteraction.Ignore
        );

        // Sceneビュー上で地面判定レイを可視化
        Debug.DrawRay(
            rayOrigin,
            Vector3.down * config.GroundCheckDistance,
            isGrounded ? Color.green : Color.red
        );

        return isGrounded;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move(float moveX, float moveZ, bool run)
    {
        float speed = run ? config.RunSpeed : config.WalkSpeed;

        Vector3 move = playerTransform.right * moveX + playerTransform.forward * moveZ;

        Vector3 velocity = move * speed;

        // 落下速度は保持
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }
}