using UnityEngine;


[CreateAssetMenu(menuName = "Items/syncWeapon")]
public class SyncWeaponItem : Item
{
    public override bool IsAvailable()
    {
        return !WeaponControllerManager.IsSyncMode;
    }

    public override void Apply()
    {
        WeaponControllerManager.IsSyncMode = true;
    }
}
