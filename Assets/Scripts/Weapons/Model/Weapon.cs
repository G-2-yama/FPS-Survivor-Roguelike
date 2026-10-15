using UnityEngine;
/// <summary>所有者の武器設定と弾数と攻撃状態を管理する</summary>

public class Weapon : MonoBehaviour
{
    public static event System.Action OnAnyFired;
    public event System.Action<Weapon> Fired;
    public event System.Action<Weapon> ReloadCompleted;

    [SerializeField] private Player weaponOwner;
    public Player WeaponOwner => weaponOwner;
    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    [SerializeField] private Transform muzzle;
    public Transform Muzzle => muzzle;

    [SerializeField] private Sounder sounder;
    public Sounder Sounder => sounder;
    [SerializeField] private WeaponView weaponView;
    public WeaponView WeaponView => weaponView;

    [SerializeField, Tooltip("照準方向と弾の生成位置に使うプレイヤーカメラのTransform")]
    private Transform aimTransform;
    public Transform AimTransform => aimTransform;

    private int currentAmmo = 0;
    public int CurrentAmmo => currentAmmo;
    
    private WeaponStateMachine stateMachine;
    public WeaponStateMachine StateMachine => stateMachine;

    private int level;
    public int Level => level;
    private WeaponLevelData CurrentLevel => weaponData.GetLevel(level);
    public bool IsEvo => weaponData.IsEvoLevel(level);
    public bool CanLevelUp => HasWeapon && level < weaponData.MaxLevelIndex;

    private WeaponRuntime runtime = new WeaponRuntime();
    public WeaponRuntime Runtime => runtime;

    private bool isReloading;
    public bool IsReloading => isReloading;

    public bool HasWeapon => !weaponData.IsEmpty;

    public int MagazineSize => Mathf.Max(1, Mathf.FloorToInt(CurrentLevel.MagazineSize * BuildModifiers().MagazineMultiplier + 0.0001f));
    public float ReloadTime => CurrentLevel.ReloadTime * BuildModifiers().ReloadTimeMultiplier;
    public int GetDamageAmount() => (int)(weaponOwner.Stats.DamageMultiplier * BuildModifiers().DamageMultiplier * CurrentLevel.Damage);
    public float GetKnockbackForce() => weaponOwner.Stats.KnockbackForceMultiplier * CurrentLevel.KnockbackForce;

    public float FireInterval => CurrentLevel.FireInterval;
    public float BurstInterval => CurrentLevel.BurstInterval;
    public float ChargeTime => CurrentLevel.ChargeTime;
    public float SpreadAngle => CurrentLevel.SpreadAngle;
    public int BurstCount => CurrentLevel.BurstCount;
    public bool AutoFire => CurrentLevel.AutoFire;
    public bool AutoReload => CurrentLevel.AutoReload;
    public WeaponTriggerType TriggerType => CurrentLevel.TriggerType;
    public FireModeData FireModeData => CurrentLevel.FireModeData;
    public Recoil Recoil => CurrentLevel.Recoil;
    public SoundDB SoundDB => CurrentLevel.SoundDB;
    public string DisplayName => CurrentLevel.DisplayName;
    public Sprite Icon => CurrentLevel.Icon;
    public GameObject WeaponModelPrefab => CurrentLevel.WeaponModelPrefab;

    private WeaponStatModifiers BuildModifiers()
    {
        WeaponStatModifiers mods = WeaponStatModifiers.Default;

        var traits = weaponData.Traits;
        for (int i = 0; i < traits.Count; i++)
        {
            if (traits[i] != null) traits[i].ModifyStats(this, ref mods);
        }

        if (weaponOwner == null)
            return mods;

        var effects = weaponOwner.WeaponEffects;
        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].ModifyStats(this, ref mods);
        }
        return mods;
    }
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        // 未設定のシーンでもカメラの補完はゲーム開始時だけに限定する
        if (aimTransform == null)
        {
            Camera camera = Camera.main;
            if (camera != null) aimTransform = camera.transform;
        }
        if (aimTransform == null) Debug.LogError("照準用Transformが設定されていません", this);
        stateMachine = new WeaponStateMachine(this);
        Equip(weaponData);
    }

    /// <summary>
    /// 新しい武器を装備するメソッド
    /// </summary>
    /// <param name="newData">新しい武器データ</param>
    /// <param name="newLevel">新しいレベル（0 始まり）</param>
    /// <param name="ammo">新しい残弾数</param>
    public void Equip(WeaponData newData, int newLevel = 0, int ammo = -1, WeaponRuntime newRuntime = null)
    {
        CancelReload();
        runtime = newRuntime ?? new WeaponRuntime();

        if (newData == null || newData.IsEmpty)
        {
            weaponData = EmptyWeaponData.Instance;
            level = 0;
            currentAmmo = 0;
        }
        else
        {
            weaponData = newData;
            level = Mathf.Clamp(newLevel, 0, weaponData.MaxLevelIndex);
            // 反動の状態は複製していない武器データのアセット上にあり、前回の再生の値（NaN を含む）が残っているため0から始める
            Recoil.Reset();

            // ammoが-1ならフルリロード、それ以外なら指定された弾数をセット
            int magazineSize = MagazineSize;
            currentAmmo = (ammo < 0) ? magazineSize : Mathf.Clamp(ammo, 0, magazineSize);
            sounder.SetSoundDB(SoundDB);
        }

        weaponView.RefreshView(this);
        stateMachine.ChangeState<WeaponIdleState>();
    }

    /// <summary>
    /// 武器レベルを上げるメソッド
    /// </summary>
    public void LevelUp()
    {
        if (!CanLevelUp)
        {
            return;
        }

        level++;
        sounder.SetSoundDB(SoundDB);
        weaponView.RefreshView(this);
    }

    public void RefreshStats()
    {
        if (!HasWeapon)
        {
            return;
        }

        int magazineSize = MagazineSize;
        currentAmmo = Mathf.Min(currentAmmo, magazineSize);
        weaponView.UpdateAmmo(currentAmmo, magazineSize);
    }

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <returns>true: 攻撃できた / false: 攻撃できなかった</returns>
    public bool Fire()
    {
        if (!HasWeapon || currentAmmo <= 0 || aimTransform == null)
        {
            return false;
        }

        currentAmmo--;
        weaponView.UpdateAmmo(currentAmmo, MagazineSize);
        FireModeData.Fire(this, weaponOwner);

        runtime.ShotsSinceReload++;
        var traits = weaponData.Traits;
        for (int i = 0; i < traits.Count; i++)
        {
            if (traits[i] != null) traits[i].OnFired(this);
        }
        if (weaponOwner != null)
        {
            var effects = weaponOwner.WeaponEffects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnFired(this);
            }
        }
        Fired?.Invoke(this);
        OnAnyFired?.Invoke();

        return true;
    }

    public float BeginReload()
    {
        isReloading = true;
        runtime.LastReloadConsumedRatio = Mathf.Clamp01(1f - (float)currentAmmo / MagazineSize);

        var ctx = new ReloadStartContext();
        var traits = weaponData.Traits;
        for (int i = 0; i < traits.Count; i++)
        {
            if (traits[i] != null) traits[i].OnReloadStart(this, ref ctx);
        }
        if (weaponOwner != null)
        {
            var effects = weaponOwner.WeaponEffects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnReloadStart(this, ref ctx);
            }
            if (ctx.LockMovement)
            {
                weaponOwner.AddMovementLock(this);
            }
        }

        return Mathf.Max(ReloadTime, 0.01f);
    }

    /// <summary>
    /// リロード処理を実装するメソッド
    /// </summary>
    public void Reload()
    {
        if (!HasWeapon)
        {
            EndReload();
            return;
        }

        var traits = weaponData.Traits;
        for (int i = 0; i < traits.Count; i++)
        {
            if (traits[i] != null) traits[i].OnReloadComplete(this);
        }
        if (weaponOwner != null)
        {
            var effects = weaponOwner.WeaponEffects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnReloadComplete(this);
            }
        }

        int magazineSize = MagazineSize;
        currentAmmo = magazineSize;
        weaponView.UpdateAmmo(currentAmmo, magazineSize);
        runtime.ShotsSinceReload = 0;

        EndReload();
        ReloadCompleted?.Invoke(this);
    }

    public void CancelReload()
    {
        if (!isReloading)
        {
            return;
        }

        var traits = weaponData.Traits;
        for (int i = 0; i < traits.Count; i++)
        {
            if (traits[i] != null) traits[i].OnReloadCanceled(this);
        }
        if (weaponOwner != null)
        {
            var effects = weaponOwner.WeaponEffects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].OnReloadCanceled(this);
            }
        }

        EndReload();
    }

    private void EndReload()
    {
        isReloading = false;
        if (weaponOwner != null) weaponOwner.RemoveMovementLock(this);
    }
}
