using UnityEngine;

public class WeaponUnlock : UpgradeBase
{
    private WeaponData target;
    private Player player;
    private bool isRightHand;

    public override bool IsAvailable()
    {
        return isRightHand ? !player.RightWeapon.HasWeapon 
                        : !player.LeftWeapon.HasWeapon;
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
            if (player.RightWeapon.HasWeapon)
            {
                Debug.Log($"Right hand weapon is already unlocked.");
                return;
            }

            player.EquipRightWeapon(target);
        }
        else
        {
            if (player.LeftWeapon.HasWeapon)
            {
                Debug.Log($"Left hand weapon is already unlocked.");
                return;
            }

            player.EquipLeftWeapon(target);
        }

        Debug.Log($"{target.DisplayName} unlocked!");
    }
}
