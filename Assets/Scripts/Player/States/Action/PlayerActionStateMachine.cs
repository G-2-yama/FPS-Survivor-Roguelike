/// <summary>
/// プレイヤーの一時アクション状態を管理するステートマシン
/// </summary>
public class PlayerActionStateMachine : StateMachine<PlayerActionState>
{
    /// <summary>
    /// 特殊動作を行っていない状態
    /// </summary>
    private PlayerNoActionState noActionState;
    public PlayerNoActionState NoActionState => noActionState;

    /// <summary>
    /// ダッシュ動作状態
    /// </summary>
    private PlayerDashActionState dashActionState;
    public PlayerDashActionState DashActionState => dashActionState;
    private PlayerSlideActionState slideActionState;
    private PlayerFastFallActionState fastFallActionState;

    /// <summary>
    /// 現在の動作ステート
    /// </summary>
    public PlayerActionState CurrentActionState => currentState;

    /// <summary>
    /// 通常移動を止めて、動作ステート側で移動を適用する状態かどうか
    /// </summary>
    public bool IsBlockingNormalMovement => currentState?.BlocksNormalMovement ?? false;

    /// <summary>
    /// 動作ステートを生成し、特殊動作なし状態へ遷移する
    /// </summary>
    public PlayerActionStateMachine(PlayerContext context)
    {
        noActionState = new PlayerNoActionState(context, this);
        dashActionState = new PlayerDashActionState(context, this);
        slideActionState = new PlayerSlideActionState(context, this);
        fastFallActionState = new PlayerFastFallActionState(context, this);
        ChangeState(noActionState);
    }

    /// <summary>
    /// 現在の動作状態を更新する前にダッシュのクールタイムを更新する
    /// </summary>
    public void Update(float deltaTime)
    {
        dashActionState.UpdateCooldown(deltaTime);
        base.Update();
    }

    /// <summary>
    /// 特殊動作を行っていない状態へ遷移する
    /// </summary>
    public void ChangeToNoActionState()
    {
        ChangeState(noActionState);
    }

    /// <summary>
    /// ダッシュ開始条件を満たしている場合にダッシュ動作へ遷移する
    /// </summary>
    public bool TryChangeToDashActionState()
    {
        if (!dashActionState.CanEnter())
        {
            return false;
        }

        ChangeState(dashActionState);
        return true;
    }

    public bool TryChangeToSlideActionState()
    {
        if (!slideActionState.CanEnter())
        {
            return false;
        }

        ChangeState(slideActionState);
        return true;
    }

    public bool TryChangeToFastFallActionState()
    {
        if (!fastFallActionState.CanEnter())
        {
            return false;
        }

        ChangeState(fastFallActionState);
        return true;
    }
}
