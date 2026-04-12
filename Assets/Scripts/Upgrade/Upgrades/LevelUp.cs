using UnityEngine;

public class LevelUp : UpgradeBase
{
    private Player player;
    private bool isRightHand;

    public override bool IsAvailable()
    {
        return isRightHand ? player.HasRightWeapon : player.HasLeftWeapon;
    }

    public LevelUp(string displayName, string description, Player player, bool isRightHand) : base(displayName, description)
    {
        this.player = player;
        this.isRightHand = isRightHand;
    }

    public override void Apply()
    {
        Weapon target = isRightHand ? player.RightWeapon : player.LeftWeapon;

        if (target == null || target.WeaponData == null)
        {
            Debug.Log("Weapon is not equipped.");
            return;
        }

        target.LevelUp();
        target.NotifyAmmoChanged();
        Debug.Log($"{target.WeaponData.DisplayName} leveled up to {target.Level}!");
    }
}
