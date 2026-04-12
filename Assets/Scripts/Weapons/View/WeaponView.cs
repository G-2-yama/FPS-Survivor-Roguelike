using UnityEngine;
using UnityEngine.UI;

public class WeaponView : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Text currentAmmoText;

    private void Start()
    {
        weapon.OnAmmoChanged += UpdateAmmo;

        if (!weapon.HasWeapon)
        {
            currentAmmoText.gameObject.SetActive(false);
            return;
        }

        currentAmmoText.gameObject.SetActive(true);
        UpdateAmmo(weapon.CurrentAmmo, weapon.WeaponStats.MagazineSize);
    }

    private void OnDestroy()
    {
        if (weapon != null)
        {
            weapon.OnAmmoChanged -= UpdateAmmo;
        }
    }

    private void UpdateAmmo(int current, int max)
    {
        if (!currentAmmoText.gameObject.activeSelf)
        {
            currentAmmoText.gameObject.SetActive(true);
        }

        currentAmmoText.text = $"{current} / {max}";
    }

}