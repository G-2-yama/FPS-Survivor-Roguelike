using UnityEngine;

public class WeaponControllerManager : MonoBehaviour
{
    [SerializeField] private WeaponController[] weaponControllers;

    /// <summary>
    /// true = 全武器同時発射 / false = 個別発射
    /// </summary>
    public static bool IsSyncMode { get; set; } = false;

    public static event System.Action OnGlobalFire;
    public static event System.Action OnGlobalFireReleased;
    public static event System.Action OnReload;

    public static void BroadcastFire()
    {
        if (IsSyncMode)
            OnGlobalFire?.Invoke();
    }

    public static void BroadcastFireReleased()
    {
        if (IsSyncMode)
            OnGlobalFireReleased?.Invoke();
    }

    public static void BroadcastReload()
    {
        if (IsSyncMode)
            OnReload?.Invoke();
    }

    public Vector2 GetTotalRecoil(float deltaTime)
    {
        Vector2 total = Vector2.zero;

        foreach (var controller in weaponControllers)
        {
            if (controller == null) continue;
            if (controller.Weapon == null) continue;
            if (controller.Weapon.WeaponRecoil == null) continue;

            var recoil = controller.Weapon.WeaponRecoil;

            recoil.Tick(deltaTime);

            total += recoil.RecoilOffset;
        }

        return total;
    }
}