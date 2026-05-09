using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Unlock")]
public class Unlock : UpgradeBase
{
    [SerializeField] private WeaponData target;

    public override bool IsAvailable()
    {
        if(target.WeaponType == WeaponType.Main)
        {
            return !player.Inventory.HasWeapon(SlotType.RightMain) || !player.Inventory.HasWeapon(SlotType.LeftMain);
        }
        else if(target.WeaponType == WeaponType.Ability)
        {
            return !player.Inventory.HasWeapon(SlotType.LeftAbility) || !player.Inventory.HasWeapon(SlotType.RightAbility);
        }
        else if(target.WeaponType == WeaponType.AutoWeapon)
        {
            return !player.Inventory.HasWeapon(SlotType.LeftAutoWeapon) || !player.Inventory.HasWeapon(SlotType.RightAutoWeapon);
        }
       
        return false;
    }

    public override void Apply()
    {
        if (target.WeaponType == WeaponType.Main)
        {
            if (!player.Inventory.HasWeapon(SlotType.LeftMain))
            {
                player.Inventory.EquipWeapon(SlotType.LeftMain, target);
            }
            else if (!player.Inventory.HasWeapon(SlotType.RightMain))
            {
                player.Inventory.EquipWeapon(SlotType.RightMain, target);
            }
        }
        else if (target.WeaponType == WeaponType.Ability)
        {
            if (!player.Inventory.HasWeapon(SlotType.LeftAbility))
            {
                player.Inventory.EquipWeapon(SlotType.LeftAbility, target);
            }
            else if (!player.Inventory.HasWeapon(SlotType.RightAbility))
            {
                player.Inventory.EquipWeapon(SlotType.RightAbility, target);
            }
        }
        else if (target.WeaponType == WeaponType.AutoWeapon)
        {
            if (!player.Inventory.HasWeapon(SlotType.LeftAutoWeapon))
            {
                player.Inventory.EquipWeapon(SlotType.LeftAutoWeapon, target);
            }
            else if (!player.Inventory.HasWeapon(SlotType.RightAutoWeapon))
            {
                player.Inventory.EquipWeapon(SlotType.RightAutoWeapon, target);
            }
        }
    }
}
