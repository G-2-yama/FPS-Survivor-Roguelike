using UnityEngine;

public class WeaponUnlock : UpgradeBase
{
    private WeaponData target;
    private Player player;
    private bool isRightHand;

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
            player.EquipRightWeapon(target);
        }
        else
        {
            player.EquipLeftWeapon(target);
        }

        Debug.Log($"{target.DisplayName} unlocked!");
    }
}
