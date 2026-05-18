using UnityEngine;
using UnityEngine.UI;

public class MainWeaponView : WeaponView
{
    [SerializeField] private Text currentAmmoText;
    [SerializeField] private Text magazineSizeText;

    protected override void Start()
    {
        base.Start();

        weapon.OnAmmoChanged += UpdateAmmo;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (weapon != null)
        {
            weapon.OnAmmoChanged -= UpdateAmmo;
        }
    }

    protected override void RefreshView()
    {
        base.RefreshView();

        bool hasWeapon = weapon.WeaponData != null;

        SetAmmoUIVisible(hasWeapon);

        if (!hasWeapon)
        {
            return;
        }

        UpdateAmmo(
            weapon.CurrentAmmo,
            weapon.WeaponStats.MagazineSize
        );
    }

    /// <summary>
    /// 弾数UIの表示切り替え
    /// </summary>
    private void SetAmmoUIVisible(bool visible)
    {
        currentAmmoText.gameObject.SetActive(visible);
        magazineSizeText.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 残弾UI更新
    /// </summary>
    private void UpdateAmmo(int current, int max)
    {
        currentAmmoText.text = current.ToString();
        magazineSizeText.text = "/" + max.ToString();
    }

    public override void PlayReloadAnimation()
    {
        animator?.SetTrigger("Reload");
    }

    public override void PlayFireAnimation()
    {
        animator?.SetTrigger("Fire");
    }
}