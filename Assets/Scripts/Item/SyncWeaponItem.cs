using UnityEngine;


[CreateAssetMenu(menuName = "Items/syncWeapon")]
public class SyncWeaponItem : Item
{
    public override bool IsAvailable()
    {
        return !player.IsWeaponSync;
    }

    public override void Apply()
    {
        player.SetWeaponSync(true);
    }

    public override void Revert()
    {
        player.SetWeaponSync(false);
    }
}
