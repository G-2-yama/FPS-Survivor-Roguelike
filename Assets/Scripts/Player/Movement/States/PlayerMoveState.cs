public abstract class PlayerMoveState : IState
{
    protected PlayerController controller;
    protected StateMachine<PlayerMoveState> moveStateMachine;

    public PlayerMoveState(PlayerController controller,
                           StateMachine<PlayerMoveState> moveStateMachine)
    {
        this.controller = controller;
        this.moveStateMachine = moveStateMachine;
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
}
