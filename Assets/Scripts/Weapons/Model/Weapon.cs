using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    [SerializeField] private int level = 0;
    public int Level => level;

    private WeaponStats weaponStats;
    public WeaponStats WeaponStats => weaponStats;

    [SerializeField] private Transform muzzle;
    public Transform Muzzle => muzzle;

    private int currentAmmo = 0;
    public int CurrentAmmo => currentAmmo;
    
    private WeaponStateMachine stateMachine;
    public WeaponStateMachine StateMachine => stateMachine;
    [SerializeField] private WeaponView weaponView;
    public WeaponView WeaponView => weaponView;

    public bool HasWeapon => weaponData != null && !weaponData.IsEmpty;

    public bool IsEmpty => !HasWeapon;

    private void Awake()
    {
        stateMachine = new WeaponStateMachine(this);
        if (!HasWeapon)
        {
            weaponData = EmptyWeaponData.Instance;
            weaponStats = WeaponStats.Empty;
            currentAmmo = 0;
            weaponView.RefreshView(this);
            return;
        }

        weaponStats = weaponData.GetStats(level);
        currentAmmo = weaponStats.MagazineSize;
    }

    /// <summary>
    /// 新しい武器を装備するメソッド
    /// </summary>
    /// <param name="newData">新しい武器データ</param>
    /// <param name="newLevel">新しいレベル</param>
    /// <param name="ammo">新しい残弾数</param>
    public void Equip(WeaponData newData, int newLevel = 0, int ammo = -1)
    {
        if (newData == null || newData.IsEmpty)
        {
            ClearWeapon();
            return;
        }
        InitializeWeapon(newData, newLevel, ammo);
    }

    /// <summary>
    /// 武器レベルを上げるメソッド
    /// </summary>
    public void LevelUp()
    {
        if (!HasWeapon)
        {
            return;
        }

        level++;
        weaponStats = weaponData.GetStats(level);
        weaponView.RefreshView(this);
    }

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <returns>true: 攻撃できた / false: 攻撃できなかった</returns>
    public bool Fire()
    {
        if (!HasWeapon || weaponStats == null || currentAmmo <= 0)
        {
            return false;
        }

        currentAmmo--;
        weaponView.RefreshView(this);
        weaponData.FireModeData.Fire(this);

        return true;
    }

    /// <summary>
    /// リロード処理を実装するメソッド
    /// </summary>
    public void Reload()
    {
        if (!HasWeapon || weaponStats == null)
        {
            return;
        }

        currentAmmo = weaponStats.MagazineSize;
        weaponView.RefreshView(this);
    }
    
    /// <summary>
    /// オートリロードをするべきかどうかを判断するメソッド
    /// </summary>
    /// <returns></returns>
    public bool ShouldStartAutoReload()
    {
        return HasWeapon &&
               weaponData.AutoReload &&
               currentAmmo <= 0;
    }

    private void InitializeWeapon(WeaponData data, int newLevel, int ammo)
    {
        weaponData = data;
        level = Mathf.Max(0, newLevel);

        weaponStats = weaponData.GetStats(level);

        // ammoが-1ならフルリロード、それ以外なら指定された弾数をセット
        currentAmmo = (ammo < 0)
            ? weaponStats.MagazineSize
            : Mathf.Clamp(ammo, 0, weaponStats.MagazineSize);

        stateMachine.ChangeIdleState();
        weaponView.RefreshView(this);
    }

    /// <summary>
    /// 武器をクリアして非装備状態にする内部処理メソッド
    /// </summary>
    private void ClearWeapon()
    {
        weaponData = EmptyWeaponData.Instance;
        level = 0;
        weaponStats = WeaponStats.Empty;
        currentAmmo = 0;

        stateMachine.ChangeIdleState();
        weaponView.RefreshView(this);
    }
}
