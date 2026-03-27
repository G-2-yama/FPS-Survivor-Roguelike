using UnityEngine;

public class PlayerMover
{
    private Transform playerTransform;
    private PlayerConfig config;

    public PlayerMover(Transform playerTransform, PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.config = config;
    }

    /// <summary>
    /// プレイヤーの移動を処理する
    /// </summary>
    /// <param name="moveInput">移動入力</param>
    /// <param name="isRunning">走行状態</param>
    public void Move(Vector2 moveInput, bool isRunning)
    {
        Vector3 move =
            playerTransform.right * moveInput.x +
            playerTransform.forward * moveInput.y;

        move.y = 0f;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        float speed = isRunning ? config.RunSpeed : config.WalkSpeed;

        playerTransform.position += move * speed * Time.deltaTime;
    }
}
