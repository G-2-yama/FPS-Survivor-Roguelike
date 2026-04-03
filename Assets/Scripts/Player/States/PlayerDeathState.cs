using UnityEngine;

public class PlayerDeathState : PlayerMoveState
{
    public PlayerDeathState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Death Stateに入りました");
    }

    /// <summary>
    /// 入力が発生したら歩行状態へ遷移
    /// </summary>
    public override void Update()
    {
        
    }
}
