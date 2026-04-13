using UnityEngine;
using System;

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

        currentAmmo = weaponData.MagazineSize;
        weaponStats = weaponData.CreateBonusStats(level);
    }

    /// <summary>
    /// 新しい武器を装備するメソッド
    /// </summary>
    /// <param name="newData">新しい武器データ</param>
    /// <param name="startLevel">開始レベル</param>
    public void Equip(WeaponData newData, int startLevel = 0)
    {
        weaponData = newData;
        level = Mathf.Max(0, startLevel);
        weaponStats = weaponData.CreateBonusStats(level);
        currentAmmo = weaponStats.MagazineSize;
        OnWeaponEquipped?.Invoke(weaponData);
        NotifyAmmoChanged();
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
        weaponStats = weaponData.CreateBonusStats(level);
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

        ExecuteFire();
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
        OnAmmoChanged?.Invoke(currentAmmo, weaponStats.MagazineSize);
    }

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    private void ExecuteFire()
    {
        var fireMode = weaponData.FireModeData;

        Vector3 direction = GetFireDirection();

        fireMode.Fire(this, direction);
    }

    /// <summary>
    /// 発射方向をスプレッド角度に基づいてランダムに決定するメソッド
    /// </summary>
    /// <returns>発射方向</returns>
    private Vector3 GetFireDirection()
    {
        if (weaponData == null)
        {
            return Vector3.zero;
        }

        float spread = weaponData.SpreadAngle;

        float x = UnityEngine.Random.Range(-spread, spread);
        float y = UnityEngine.Random.Range(-spread, spread);

        Vector3 direction = Camera.main.transform.forward;
        direction = Quaternion.Euler(y, x, 0) * direction;

        return direction;
    }

}
