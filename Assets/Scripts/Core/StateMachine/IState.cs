public interface IState
{
    /// <summary>
    /// 状態に入る際に一度だけ呼び出し
    /// </summary>
    void Enter();

    /// <summary>
    /// 状態が有効な間、毎フレーム呼び出し
    /// </summary>
    void Update();

    /// <summary>
    /// 状態から離れる際に一度だけ呼び出し
    /// </summary>
    void Exit();
}
