using UnityEngine;

public class speedupitem : PickupTriggerItem
{
    [SerializeField] float upspeedrate = 5f;
    
    protected override void OnPickup(Player player)
    {
        player.Stats.AddRunSpeed(upspeedrate);
        player.Stats.AddWalkSpeed(upspeedrate);
        player.Stats.AddJumpForce(upspeedrate);
    }
}
