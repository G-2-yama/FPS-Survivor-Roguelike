using UnityEngine;

public class healingHpitem : PickupTriggerItem
{
    [SerializeField] private int healamount = 300;
    public float HealAmount => healamount;

    protected override void OnPickup(Player player)
    {
        player.Health.Heal(healamount);

    }

}
