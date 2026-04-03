public abstract class PlayerMoveState : IState
{
    protected PlayerController controller;

    public PlayerMoveState(PlayerController controller)
    {
        this.controller = controller;
    }

    /// <summary>
    /// 状態開始時の初期化処理
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// 状態のフレーム更新処理
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// 状態終了時の後処理
    /// </summary>
    public virtual void Exit() { }

    /// <summary>
    /// 視点処理を許可するか
    /// </summary>
    public virtual bool AllowLook => true;

    /// <summary>
    /// 移動処理を許可するか
    /// </summary>
    public virtual bool AllowMove => true;
}
