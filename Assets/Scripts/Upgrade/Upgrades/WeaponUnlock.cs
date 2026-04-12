using UnityEngine;

public class WeaponUnlock : UpgradeBase
{
    private WeaponData target;
    private Player player;
    private bool isRightHand;

    public override bool IsAvailable()
    {
        return isRightHand ? !player.HasRightWeapon : !player.HasLeftWeapon;
    }

    public WeaponUnlock(string displayName, string description, WeaponData target, Player player, bool isRightHand) : base(displayName, description)
    {
        this.target = target;
        this.player = player;
        this.isRightHand = isRightHand;
    }

    public override void Apply()
    {
        if(isRightHand)
        {
            if (player.HasRightWeapon)
            {
                Debug.Log($"Right hand weapon is already unlocked.");
                return;
            }

            player.EquipRightWeapon(target);
        }
        else
        {
            if (player.HasLeftWeapon)
            {
                Debug.Log($"Left hand weapon is already unlocked.");
                return;
            }

            player.EquipLeftWeapon(target);
        }

        Debug.Log($"{target.DisplayName} unlocked!");
    }
}
