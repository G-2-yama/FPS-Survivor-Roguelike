using UnityEngine;

public class AbilityUnlock : UpgradeBase
{
    private WeaponData target;
    private Player player;
    private bool isRightHand;

    public override bool IsAvailable()
    {
        return isRightHand ? !player.HasRightAbility : !player.HasLeftAbility;
    }

    public AbilityUnlock(string displayName, string description, WeaponData target, Player player, bool isRightHand) : base(displayName, description)
    {
        this.target = target;
        this.player = player;
        this.isRightHand = isRightHand;
    }

    public override void Apply()
    {
        if(isRightHand)
        {
            if (player.HasRightAbility)
            {
                Debug.Log($"Right hand ability is already unlocked.");
                return;
            }

            player.EquipRightAbility(target);
        }
        else
        {
            if (player.HasLeftAbility)
            {
                Debug.Log($"Left hand ability is already unlocked.");
                return;
            }

            player.EquipLeftAbility(target);
        }

        Debug.Log($"{target.DisplayName} unlocked!");
    }
}
