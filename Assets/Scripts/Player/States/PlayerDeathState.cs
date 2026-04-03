using UnityEngine;

public class PlayerDeathState : PlayerMoveState
{
    public PlayerDeathState(PlayerController controller) : base(controller) { }

    public override bool AllowLook => false;
    public override bool AllowMove => false;

    public override void Enter()
    {
        Debug.Log("Death Stateに入りました");
    }

    /// <summary>
    /// 死亡中は行動せず待機
    /// </summary>
    public override void Update()
    {
        
    }
}
