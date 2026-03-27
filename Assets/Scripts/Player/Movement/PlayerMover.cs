using UnityEngine;

/// <summary>
/// プレイヤーの水平移動・傾斜追従・重力処理を担当するクラス
/// </summary>
public class PlayerMover
{
    /// <summary>
    /// プレイヤー本体のTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// 移動設定値を保持する参照
    /// </summary>
    private PlayerConfig config;

    /// <summary>
    /// 現在の縦方向速度
    /// </summary>
    private float verticalVelocity;

    /// <summary>
    /// PlayerMoverを初期化する
    /// </summary>
    /// <param name="playerTransform">移動対象となるプレイヤーのTransform</param>
    /// <param name="config">移動・重力に関する設定</param>
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

        // 斜め移動でも速度が速くなりすぎないように入力の大きさを正規化
        float inputMagnitude = Mathf.Clamp01(move.magnitude);
        bool hasMoveInput = inputMagnitude > 0f;

        if (hasMoveInput)
        {
            move.Normalize();

            if (TryGetGroundNormal(out Vector3 groundNormal))
            {
                move = Vector3.ProjectOnPlane(move, groundNormal).normalized;
            }
            else // 水平移動の場合
            {
                move.y = 0f;
                move.Normalize();
            }

            // 入力の大きさを移動量に反映
            move *= inputMagnitude;
        }
        else
        {
            move = Vector3.zero;
        }

        UpdateVerticalVelocity();

        float speed = isRunning ? config.RunSpeed : config.WalkSpeed;
        Vector3 frameMove = move * speed;
        frameMove.y = verticalVelocity;

        Vector3 nextPosition = playerTransform.position + frameMove * Time.deltaTime;

        if (TryGetGroundHit(nextPosition, out RaycastHit groundHit) && verticalVelocity <= 0f)
        {
            nextPosition.y = groundHit.point.y;
            verticalVelocity = config.GroundedVerticalVelocity;
        }

        playerTransform.position = nextPosition;
    }

    /// <summary>
    /// 現在フレームで接地しているかを判定する
    /// </summary>
    /// <returns>接地していればtrue、空中であればfalse</returns>
    public bool IsGrounded()
    {
        return TryGetGroundHit(playerTransform.position, out _);
    }

    /// <summary>
    /// 接地状態に応じて縦方向速度を更新する
    /// </summary>
    private void UpdateVerticalVelocity()
    {
        if (TryGetGroundHit(playerTransform.position, out RaycastHit groundHit) && verticalVelocity <= 0f)
        {
            verticalVelocity = config.GroundedVerticalVelocity;
            return;
        }

        verticalVelocity += config.Gravity * Time.deltaTime;
    }

    /// <summary>
    /// 足元の地面法線を取得する
    /// </summary>
    /// <param name="groundNormal">取得した地面法線</param>
    /// <returns>地面を検知できた場合はtrue</returns>
    private bool TryGetGroundNormal(out Vector3 groundNormal)
    {
        if (TryGetGroundHit(playerTransform.position, out RaycastHit hit))
        {
            groundNormal = hit.normal;
            return true;
        }

        groundNormal = Vector3.up;
        return false;
    }

    /// <summary>
    /// 指定位置から下方向へレイを飛ばして地面ヒット情報を取得する
    /// </summary>
    /// <param name="referencePosition">判定の基準位置</param>
    /// <param name="hit">ヒットした地面情報</param>
    /// <returns>地面を検知できた場合はtrue</returns>
    private bool TryGetGroundHit(Vector3 referencePosition, out RaycastHit hit)
    {
        Vector3 rayOrigin = referencePosition + Vector3.up * config.GroundRayStartOffset;
        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            config.GroundCheckDistance,
            config.GroundLayers,
            QueryTriggerInteraction.Ignore);
    }
}
