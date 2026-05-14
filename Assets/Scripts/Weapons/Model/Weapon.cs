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

    /// <summary>
    /// 弾薬数が変化したときに通知するイベント
    /// </summary>
    public event Action<int, int> OnAmmoChanged;

    /// <summary>
    /// 武器が装備されたときに通知するイベント
    /// </summary>
    public event Action<WeaponData> OnWeaponEquipped;

    private void Awake()
    {
        if (weaponData == null)
        {
            return;
        }

        weaponStats = weaponData.CreateStats(level);
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
        if (newData == null)
        {
            ClearWeapon();
            return;
        }

        ApplyWeapon(newData, newLevel, ammo);
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
        weaponStats = weaponData.CreateStats(level);
    }

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <returns>true: 攻撃できた / false: 攻撃できなかった</returns>
    public bool Fire()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log($"{weaponData.DisplayName} is out of ammo!");
            return false;
        }

        currentAmmo--;
        NotifyAmmoChanged();

        var fireMode = weaponData.FireModeData;
        fireMode.Fire(this);

        return true;
    }

    /// <summary>
    /// リロード処理を実装するメソッド
    /// </summary>
    public void Reload()
    {
        currentAmmo = weaponStats.MagazineSize;
        NotifyAmmoChanged();
    }

    /// <summary>
    /// 弾薬数の変化を通知するメソッド
    /// </summary>
    public void NotifyAmmoChanged()
    {
        int maxAmmo = weaponStats != null ? weaponStats.MagazineSize : 0;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }
    
    /// <summary>
    /// オートリロードをするべきかどうかを判断するメソッド
    /// </summary>
    /// <returns></returns>
    public bool ShouldStartAutoReload()
    {
        if(WeaponData == null)
        {
            return false;
        }
        
        return WeaponData.AutoReload && CurrentAmmo <= 0;
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

        OnWeaponEquipped?.Invoke(null);
        NotifyAmmoChanged();
    }

    /// <summary>
    /// 新しい武器を装備する内部処理メソッド
    /// </summary>
    /// <param name="newData">新しい武器データ</param>
    /// <param name="newLevel">新しいレベル</param>
    /// <param name="ammo">新しい残弾数</param>
    private void ApplyWeapon(WeaponData newData, int newLevel, int ammo)
    {
        weaponData = newData;
        level = Mathf.Max(0, newLevel);
        weaponStats = weaponData.CreateStats(level);

        // ammoが-1ならフルリロード、それ以外なら指定された弾数をセット
        currentAmmo = ammo == -1
            ? weaponStats.MagazineSize
            : Mathf.Clamp(ammo, 0, weaponStats.MagazineSize);

        OnWeaponEquipped?.Invoke(weaponData);
        NotifyAmmoChanged();
    }

}
