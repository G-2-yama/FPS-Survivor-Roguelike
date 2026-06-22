using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Boomerang")]
public class BoomerangFire : FireModeData
{
    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxDistance = 8f;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        GameObject obj = PoolManager.Instance.Get(boomerangPrefab);

        Vector3 direction = GetFireDirection(weapon);

        var boomerang = obj.GetComponent<BoomerangDamageArea>();

        boomerang.Initialize(
            GetDamageAmount(weapon, weaponOwner),
            GetKnockbackForce(weapon, weaponOwner),
            weaponOwner.transform,
            direction,
            speed,
            maxDistance);
    }
}