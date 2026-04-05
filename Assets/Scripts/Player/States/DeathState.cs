using UnityEngine;

public class DeathState : PlayerState
{
    public DeathState(PlayerController controller) : base(controller) { }

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
