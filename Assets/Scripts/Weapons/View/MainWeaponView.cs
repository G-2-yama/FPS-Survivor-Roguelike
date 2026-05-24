using UnityEngine;
using UnityEngine.UI;

public class MainWeaponView : WeaponView
{
    [SerializeField] private Text currentAmmoText;
    [SerializeField] private Text magazineSizeText;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void RefreshView(Weapon weapon)
    {
        base.RefreshView(weapon);

        SetAmmoUIVisible(weapon != null && weapon.HasWeapon);

        if (weapon == null || !weapon.HasWeapon)
            return;

        UpdateAmmo(
            weapon.CurrentAmmo,
            weapon.WeaponStats.MagazineSize
        );
    }

    public override void PlayReloadAnimation()
    {
        animator?.SetTrigger("Reload");
    }

    public override void PlayFireAnimation()
    {
        animator?.SetTrigger("Fire");
    }

    private void SetAmmoUIVisible(bool visible)
    {
        currentAmmoText.gameObject.SetActive(visible);
        magazineSizeText.gameObject.SetActive(visible);
    }

    private void UpdateAmmo(int current, int max)
    {
        currentAmmoText.text = current.ToString();
        magazineSizeText.text = "/" + max;
    }
}