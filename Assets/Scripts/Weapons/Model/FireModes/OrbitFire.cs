using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Orbit")]
public class OrbitFireData : FireModeData
{
    [SerializeField] GameObject orbitPrefab;

    [SerializeField] float radius = 3f;
    [SerializeField] float rotateSpeed = 180f;
    [SerializeField] int areaCount = 1;

    public override void Fire(Weapon weapon,Player weaponOwner)
    {
        for (int i = 0; i < areaCount; i++)
        {
            float angle = 360f / areaCount * i;
            GameObject obj =PoolManager.Instance.Get(orbitPrefab);

            obj.transform.position = weaponOwner.transform.position;

            OrbitDamageArea orbit = obj.GetComponent<OrbitDamageArea>();

            orbit.Initialize(GetDamageAmount(weapon, weaponOwner),
                GetKnockbackForce(weapon, weaponOwner),
                weaponOwner.transform, radius, rotateSpeed, angle);
        }
    }
}