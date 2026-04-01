using UnityEngine;
using System;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;

    public WeaponData WeaponData => weaponData;

    private int currentAmmo = 0;
    public int CurrentAmmo => currentAmmo;

    /// <summary>
    /// 弾薬数が変化したときに通知するイベント
    /// </summary>
    public event Action<int, int> OnAmmoChanged;

    private void Start()
    {
        currentAmmo = weaponData.MagazineSize;
    }

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    public void Fire()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            NotifyAmmoChanged();
        }
        else
        {
            Debug.Log($"{weaponData.DisplayName} is out of ammo!");
        }
    }

    /// <summary>
    /// リロード処理を実装するメソッド
    /// </summary>
    public void Reload()
    {
        currentAmmo = weaponData.MagazineSize;
        NotifyAmmoChanged();
    }

    /// <summary>
    /// 弾薬数の変化を通知するメソッド
    /// </summary>
    private void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmo, weaponData.MagazineSize);
    }
}
