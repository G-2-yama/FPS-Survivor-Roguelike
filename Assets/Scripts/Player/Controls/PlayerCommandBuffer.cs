/// <summary>
/// 1回だけ消費するプレイヤー操作要求を蓄える。
/// State ではなく CommandBuffer として切り出し、状態表現と混同しないようにしている。
/// </summary>
public class PlayerCommandBuffer
{
    /// <summary>
    /// 未処理のジャンプ要求数
    /// </summary>
    private int pendingJumpRequests;

    /// <summary>
    /// 未処理のダッシュ要求数
    /// </summary>
    private int pendingDashRequests;
    private int pendingCrouchActionRequests;
    private bool slideOnNextLandArmed;

    /// <summary>
    /// ジャンプ要求を1件追加する
    /// </summary>
    public void EnqueueJump()
    {
        pendingJumpRequests++;
    }

    /// <summary>
    /// ダッシュ要求を1件追加する
    /// </summary>
    public void EnqueueDash()
    {
        pendingDashRequests++;
    }

    public void EnqueueCrouchAction()
    {
        pendingCrouchActionRequests++;
    }

    public void ArmSlideOnNextLand()
    {
        slideOnNextLandArmed = true;
    }

    public void DisarmSlideOnNextLand()
    {
        slideOnNextLandArmed = false;
    }

    /// <summary>
    /// ジャンプ要求を1件消費する
    /// </summary>
    public bool TryConsumeJump()
    {
        if (pendingJumpRequests <= 0)
        {
            return false;
        }

        pendingJumpRequests--;
        return true;
    }

    /// <summary>
    /// ダッシュ要求を1件消費する
    /// </summary>
    public bool TryConsumeDash()
    {
        if (pendingDashRequests <= 0)
        {
            return false;
        }

        pendingDashRequests--;
        return true;
    }

    public bool TryConsumeCrouchAction()
    {
        if (pendingCrouchActionRequests <= 0)
        {
            return false;
        }

        pendingCrouchActionRequests--;
        return true;
    }

    public bool TryConsumeSlideOnNextLand(bool isReadyToConsume)
    {
        if (!slideOnNextLandArmed || !isReadyToConsume)
        {
            return false;
        }

        slideOnNextLandArmed = false;
        return true;
    }

    /// <summary>
    /// 未処理の要求をすべて破棄する
    /// </summary>
    public void Reset()
    {
        pendingJumpRequests = 0;
        pendingDashRequests = 0;
        pendingCrouchActionRequests = 0;
        slideOnNextLandArmed = false;
    }
}
