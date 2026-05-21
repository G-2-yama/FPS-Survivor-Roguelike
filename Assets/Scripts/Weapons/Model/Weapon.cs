using UnityEngine;
using System;
using Unity.VisualScripting;

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

    private WeaponRecoil weaponRecoil;
    public WeaponRecoil WeaponRecoil => weaponRecoil;
    
    private WeaponStateMachine stateMachine;
    public WeaponStateMachine StateMachine => stateMachine;
    [SerializeField] private WeaponView weaponView;
    public WeaponView WeaponView => weaponView;

    public bool HasWeapon => weaponData != null;

    private void Awake()
    {
        stateMachine = new WeaponStateMachine(this);
        weaponRecoil = new WeaponRecoil();
        if (weaponData == null)
        {
            return;
        }

        weaponStats = weaponData.GetStats(level);
        currentAmmo = weaponStats.MagazineSize;

        weaponRecoil.Initialization(weaponStats.RecoilProfile);
    }

    /// <summary>
    /// 新しい武器を装備するメソッド
    /// </summary>
    /// <param name="newData">新しい武器データ</param>
    /// <param name="newLevel">新しいレベル</param>
    /// <param name="ammo">新しい残弾数</param>
    public void Equip(WeaponData newData, int newLevel = 0, int ammo = -1)
    {
        if (newData == null)
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
        if (weaponData == null)
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
        if (currentAmmo <= 0)
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
        currentAmmo = weaponStats.MagazineSize;
        weaponView.RefreshView(this);
    }
    
    /// <summary>
    /// オートリロードをするべきかどうかを判断するメソッド
    /// </summary>
    /// <returns></returns>
    public bool ShouldStartAutoReload()
    {
        return weaponData != null &&
               weaponData.AutoReload &&
               currentAmmo <= 0;
    }

    private void InitializeWeapon(WeaponData data, int newLevel, int ammo)
    {
        weaponData = data;
        level = Mathf.Max(0, newLevel);

        weaponStats = weaponData.GetStats(level);

        weaponRecoil.Initialization(weaponStats.RecoilProfile);

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
        weaponData = null;
        level = 0;
        weaponStats = null;
        currentAmmo = 0;

        stateMachine.ChangeIdleState();
        weaponView.RefreshView(this);
    }
}
