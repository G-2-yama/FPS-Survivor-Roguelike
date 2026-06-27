using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Boomerang")]
public class BoomerangFire : FireModeData
{
    [SerializeField] private GameObject boomerangPrefab;

    [SerializeField] private int boomerangCount = 1;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 center = weaponOwner.transform.position;
        Vector3 baseDirection = GetFireDirection(weapon);

        for (int i = 0; i < boomerangCount; i++)
        {
            float angle = i * 360f / boomerangCount;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;

            GameObject obj = PoolManager.Instance.Get(boomerangPrefab);

            BoomerangDamageArea boomerang = obj.GetComponent<BoomerangDamageArea>();

            boomerang.Initialize(
                GetDamageAmount(weapon, weaponOwner),
                GetKnockbackForce(weapon, weaponOwner),
                weaponOwner.transform,
                direction
            );
        }
    }
}