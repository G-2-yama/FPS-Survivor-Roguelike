using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Orbit")]
public class OrbitFireData : FireModeData
{
    [SerializeField] GameObject orbitPrefab;

    [SerializeField] float radius = 3f;
    [SerializeField] float rotateSpeed = 180f;

    public override void Fire(Weapon weapon,Player weaponOwner)
    {
        GameObject obj =PoolManager.Instance.Get(orbitPrefab);

        obj.transform.position = weaponOwner.transform.position;

        OrbitDamageArea orbit = obj.GetComponent<OrbitDamageArea>();

        orbit.Initialize(GetDamageAmount(weapon, weaponOwner),
            GetKnockbackForce(weapon, weaponOwner),
            weaponOwner.transform, radius, rotateSpeed);
    }
}