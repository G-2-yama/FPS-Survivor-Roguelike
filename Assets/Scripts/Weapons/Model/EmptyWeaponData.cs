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

    public override bool IsEmpty => true;

    private static EmptyWeaponData instance;
}