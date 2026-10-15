using UnityEngine;

[CreateAssetMenu(fileName = "StationaryReloadTrait", menuName = "Attributes/Traits/Stationary Reload")]
public class StationaryReloadTrait : WeaponTrait
{
    [Tooltip("立ち止まってリロードを始めたときのリロード時間の倍率")]
    [SerializeField, Min(0f)] private float reloadTimeMultiplier = 0.5f;

    public override void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods)
    {
        if (IsStationary(weapon))
            mods.ReloadTimeMultiplier *= reloadTimeMultiplier;
    }

    public override void OnReloadStart(Weapon weapon, ref ReloadStartContext ctx)
    {
        if (IsStationary(weapon))
            ctx.LockMovement = true;
    }

    private static bool IsStationary(Weapon weapon) => weapon.WeaponOwner != null && weapon.WeaponOwner.IsStationary;
}
