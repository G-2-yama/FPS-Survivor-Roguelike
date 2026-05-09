using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/LevelUp")]
public class LevelUp : UpgradeBase
{
    [SerializeField] private SlotType targetType;

    public override bool IsAvailable()
    {
        return player.Inventory.HasWeapon(targetType);
    }

    public override void Apply()
    {
        Weapon target = player.Inventory.Slots[targetType];

        target.LevelUp();
        target.NotifyAmmoChanged();
    }
}
