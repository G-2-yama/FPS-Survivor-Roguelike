public static class WeaponControllerManager
{
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
}