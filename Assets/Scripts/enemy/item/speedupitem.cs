using UnityEngine;

public class speedupitem : PickupTriggerItem
{
    [SerializeField] float upspeedrate = 100f;
    [SerializeField] float duration = 3f;

    protected override void OnPickup(Player player)
    {
        player.Sounder.Play(SoundCategory.GetItem, 7);
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
