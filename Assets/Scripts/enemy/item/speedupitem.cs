using UnityEngine;

public class speedupitem : PickupTriggerItem
{
    [SerializeField] float upspeedrate = 5f;
    [SerializeField] float duration = 5f;

    protected override void OnPickup(Player player)
    {
        TimedBuffManager timedBuffManager = player.GetComponent<TimedBuffManager>();
        TimedBuff buff = new TimedBuff(
            duration,
            () =>
            {
                player.Stats.AddRunSpeed(upspeedrate);
                player.Stats.AddWalkSpeed(upspeedrate);
                player.Stats.AddJumpForce(upspeedrate);
            },
            () =>
            {
                player.Stats.AddRunSpeed(-upspeedrate);
                player.Stats.AddWalkSpeed(-upspeedrate);
                player.Stats.AddJumpForce(-upspeedrate);
            }
        );
        timedBuffManager.AddBuff(buff);
    }
}
