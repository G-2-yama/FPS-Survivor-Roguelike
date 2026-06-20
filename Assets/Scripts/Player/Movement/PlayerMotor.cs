using UnityEngine;

/// <summary>
/// CharacterControllerへの速度適用と接触補正を扱うクラス
/// </summary>
public class PlayerMotor
{
    /// <summary>
    /// 移動計算に使用するCharacterController
    /// </summary>
    private CharacterController characterController;

    /// <summary>
    /// 水平方向の現在速度
    /// </summary>
    public Vector3 HorizontalVelocity { get; set; }

    /// <summary>
    /// 垂直方向の現在速度
    /// </summary>
    public float VerticalVelocity { get; set; }
    public float ControllerHeight => characterController.height;
    public Vector3 ControllerCenter => characterController.center;

    /// <summary>
    /// 移動処理に必要な参照を初期化する
    /// </summary>
    /// <param name="characterController">移動を適用するCharacterController</param>
    public PlayerMotor(CharacterController characterController)
    {
        this.characterController = characterController;
    }

    /// <summary>
    /// 現在速度をCharacterControllerへ適用する
    /// </summary>
    /// <param name="deltaTime">移動を適用する時間</param>
    public void Move(float deltaTime)
    {
        Vector3 velocity = HorizontalVelocity + Vector3.up * VerticalVelocity;
        characterController.Move(velocity * deltaTime);
    }

    /// <summary>
    /// 指定された水平速度と現在の垂直速度をCharacterControllerへ適用する
    /// </summary>
    /// <param name="horizontalVelocity">一時的に適用する水平速度</param>
    /// <param name="deltaTime">移動を適用する時間</param>
    public void MoveWithHorizontalVelocity(Vector3 horizontalVelocity, float deltaTime)
    {
        Vector3 velocity = horizontalVelocity + Vector3.up * VerticalVelocity;
        characterController.Move(velocity * deltaTime);
    }

    /// <summary>
    /// CharacterControllerの接地状態を取得する
    /// </summary>
    /// <returns>接地している場合はtrue</returns>
    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    /// <summary>
    /// 壁面へ向かう水平速度を壁に沿う方向へ補正する
    /// </summary>
    /// <param name="wallNormal">接触した壁の法線</param>
    public void ResolveWallHit(Vector3 wallNormal)
    {
        if (wallNormal.y > 0.1f)
        {
            return;
        }

        HorizontalVelocity = Vector3.ProjectOnPlane(HorizontalVelocity, wallNormal);
    }

    /// <summary>
    /// 移動速度を停止状態へ戻す
    /// </summary>
    public void Stop()
    {
        HorizontalVelocity = Vector3.zero;
        VerticalVelocity = 0f;
    }

    public void SetControllerHeight(float newHeight, bool keepBottomAligned = true)
    {
        newHeight = Mathf.Max(newHeight, characterController.radius * 2f);

        Vector3 center = characterController.center;
        if (keepBottomAligned)
        {
            float bottom = center.y - (characterController.height * 0.5f);
            center.y = bottom + (newHeight * 0.5f);
        }

        characterController.height = newHeight;
        characterController.center = center;
    }

    public void SetControllerDimensions(float height, Vector3 center)
    {
        characterController.height = Mathf.Max(height, characterController.radius * 2f);
        characterController.center = center;
    }
}
