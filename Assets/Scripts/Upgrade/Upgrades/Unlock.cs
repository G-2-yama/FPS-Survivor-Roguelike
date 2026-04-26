using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Unlock")]
public class Unlock : UpgradeBase
{
    [SerializeField] private WeaponData target;

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

    public override void Apply()
    {
        if (target.WeaponType == WeaponType.Main)
        {
            if (!player.HasLeftWeapon)
            {
                player.EquipLeftWeapon(target);
            }
            else if (!player.HasRightWeapon)
            {
                player.EquipRightWeapon(target);
            }
        }
        else if (target.WeaponType == WeaponType.Ability)
        {
            if (!player.HasLeftAbility)
            {
                player.EquipLeftAbility(target);
            }
            else if (!player.HasRightAbility)
            {
                player.EquipRightAbility(target);
            }
        }
        else if (target.WeaponType == WeaponType.AutoWeapon)
        {
            if (!player.HasLeftAutoWeapon)
            {
                player.EquipLeftAutoWeapon(target);
            }
            else if (!player.HasRightAutoWeapon)
            {
                player.EquipRightAutoWeapon(target);
            }
        }
    }
}
