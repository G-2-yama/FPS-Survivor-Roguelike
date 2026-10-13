using UnityEngine;
/// <summary>プレイヤーの移動能力を一定時間だけ強化する</summary>

public class speedupitem : PickupTriggerItem
{
    /// <summary>接触したプレイヤーへアイテムの取得効果を適用する</summary>
    [SerializeField] float upspeedrate = 100f;
    [SerializeField] float duration = 3f;

    protected override void OnPickup(Player player)
    {
        player.Sounder.Play(SoundCategory.GetItem, 7);
        TimedBuffManager timedBuffManager = player.TimedBuffManager;
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
