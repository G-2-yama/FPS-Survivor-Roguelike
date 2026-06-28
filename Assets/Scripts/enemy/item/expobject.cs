using UnityEngine;

public class Exp : PickupTriggerItem
{
    [SerializeField] private float expAmount = 1;
    public float ExpAmount => expAmount;

    protected override void OnPickup(Player player)
    {
        player.AddExp(ExpAmount);

    }





}