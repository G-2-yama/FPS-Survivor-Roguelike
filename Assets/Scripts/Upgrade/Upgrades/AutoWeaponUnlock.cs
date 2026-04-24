using UnityEngine;

public class AutoWeaponUnlock : UpgradeBase
{
    private WeaponData target;
    private Player player;
    private bool isRightHand;

    public override bool IsAvailable()
    {
        return isRightHand ? !player.HasRightAutoWeapon : !player.HasLeftAutoWeapon;
    }

    public AutoWeaponUnlock(string displayName, string description, WeaponData target, Player player, bool isRightHand) : base(displayName, description)
    {
        this.target = target;
        this.player = player;
        this.isRightHand = isRightHand;
    }

    public override void Apply()
    {
        if(isRightHand)
        {
            if (player.HasRightAutoWeapon)
            {
                Debug.Log($"Right hand auto weapon is already unlocked.");
                return;
            }

            player.EquipRightAutoWeapon(target);
        }
        else
        {
            if (player.HasLeftAutoWeapon)
            {
                Debug.Log($"Left hand auto weapon is already unlocked.");
                return;
            }

            player.EquipLeftAutoWeapon(target);
        }

        Debug.Log($"{target.DisplayName} unlocked!");
    }
}
