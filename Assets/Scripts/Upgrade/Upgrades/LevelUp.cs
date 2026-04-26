using UnityEngine;

public class LevelUp : UpgradeBase
{
    private Player player;
    private WeaponType targetType;
    private bool isRightHand;


    public override bool IsAvailable()
    {
        if (targetType == WeaponType.Main)
        {
            if(isRightHand)
                return player.HasRightWeapon;
            else
                return player.HasLeftWeapon;
        }
        else if (targetType == WeaponType.Ability)
        {
            if (isRightHand)
                return player.HasRightAbility;
            else
                return player.HasLeftAbility;
        }
        else if (targetType == WeaponType.AutoWeapon)
        {
            if (isRightHand)
                return player.HasRightAutoWeapon;
            else
                return player.HasLeftAutoWeapon;
        }

        return false;
    }

    public LevelUp(string displayName, string description,WeaponType weaponType ,Player player, bool isRightHand) : base(displayName, description)
    {
        this.targetType = weaponType;
        this.player = player;
        this.isRightHand = isRightHand;
    }

    public override void Apply()
    {
        Weapon target = null;

        if(targetType == WeaponType.Main)
        {
            if (isRightHand)
                target = player.RightWeapon;
            else
                target = player.LeftWeapon;
        }
        else if(targetType == WeaponType.Ability)
        {
            if (isRightHand)
                target = player.RightAbility;
            else
                target = player.LeftAbility;
        }
        else if(targetType == WeaponType.AutoWeapon)
        {
            if (isRightHand)
                target = player.RightAutoWeapon;
            else
                target = player.LeftAutoWeapon;
        }

        if(target != null)
        {
            return;
        }

        target.LevelUp();
        target.NotifyAmmoChanged();
        Debug.Log($"{target.WeaponData.DisplayName} leveled up to {target.Level}!");
    }
}
