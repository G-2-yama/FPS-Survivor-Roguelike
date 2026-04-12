using UnityEngine;

public class LevelUp : UpgradeBase
{
    private Weapon target;

    public override bool IsAvailable()
    {
        return target.HasWeapon;
    }

    public LevelUp(string displayName, string description, Weapon target) : base(displayName, description)
    {
        this.target = target;
    }

    public override void Apply()
    {
        target.LevelUp();
        target.NotifyAmmoChanged();
        Debug.Log($"{target.WeaponData.DisplayName} leveled up to {target.Level}!");
    }
}
