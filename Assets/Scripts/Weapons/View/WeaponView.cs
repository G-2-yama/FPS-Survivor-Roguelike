using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponView : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Text currentAmmoText;

    private void Start()
    {
        weapon.OnAmmoChanged += UpdateAmmo;
        UpdateAmmo(weapon.CurrentAmmo, weapon.WeaponStats.MagazineSize);
    }

    private void UpdateAmmo(int current, int max)
    {
        currentAmmoText.text = $"{current} / {max}";
    }



}