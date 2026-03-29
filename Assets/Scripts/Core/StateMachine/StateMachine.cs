public class StateMachine<T> where T : IState
{
    /// <summary>
    /// 現在アクティブな状態を保持
    /// </summary>
    protected T currentState;

    /// <summary>
    /// 現在の状態を終了し、新しい状態へ切り替え
    /// </summary>
    /// <param name="newState">遷移先の状態</param>
    public void ChangeState(T newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
    
    /// <summary>
    /// 現在の状態の更新処理を実行
    /// </summary>
    public void Update()
    {
        currentState?.Update();
    }
}
