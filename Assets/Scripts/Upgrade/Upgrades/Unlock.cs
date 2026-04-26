using UnityEngine;

public class Unlock : UpgradeBase
{
    private WeaponData target;
    private Player player;
    private bool isRightHand;

    public override bool IsAvailable()
    {
        if(target.WeaponType == WeaponType.Main)
        {
            return !player.HasLeftWeapon || !player.HasRightWeapon;
        }
        else if(target.WeaponType == WeaponType.Ability)
        {
            return !player.HasLeftAbility || !player.HasRightAbility;
        }
        else if(target.WeaponType == WeaponType.AutoWeapon)
        {
            return !player.HasLeftAutoWeapon || !player.HasRightAutoWeapon;
        }
       
        return false;
    }

    public Unlock(string displayName, string description, WeaponData target, Player player, bool isRightHand) : base(displayName, description)
    {
        this.target = target;
        this.player = player;
        this.isRightHand = isRightHand;
    }

    public override void Apply()
    {
        if (target.WeaponType == WeaponType.Main)
        {
            if (isRightHand && !player.HasRightWeapon)
            {
                player.EquipRightWeapon(target);
            }
            else if (!isRightHand && !player.HasLeftWeapon)
            {
                player.EquipLeftWeapon(target);
            }
        }
        else if (target.WeaponType == WeaponType.Ability)
        {
            if(isRightHand && !player.HasRightAbility)
            {
                player.EquipRightAbility(target);
            }
            else if (!isRightHand && !player.HasLeftAbility)
            {
                player.EquipLeftAbility(target);
            }
        }
        else if (target.WeaponType == WeaponType.AutoWeapon)
        {
            if (isRightHand && !player.HasRightAutoWeapon)
            {
                player.RightAutoWeapon.Equip(target);
            }
            else if (!isRightHand && !player.HasLeftAutoWeapon)
            {
                player.LeftAutoWeapon.Equip(target);
            }
        }
    }



}
