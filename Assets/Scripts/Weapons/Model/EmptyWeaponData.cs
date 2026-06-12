using UnityEngine;

[CreateAssetMenu(fileName = "EmptyWeaponData", menuName = "Weapons/Empty Weapon Data")]
public sealed class EmptyWeaponData : WeaponData
{
    public static EmptyWeaponData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = CreateInstance<EmptyWeaponData>();
                instance.name = nameof(EmptyWeaponData);
                instance.hideFlags = HideFlags.HideAndDontSave;
            }

            return instance;
        }
    }


    private static EmptyWeaponData instance;
}