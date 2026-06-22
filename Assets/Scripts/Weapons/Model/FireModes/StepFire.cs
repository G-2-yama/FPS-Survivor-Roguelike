using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/StepFire")]
public class StepFire : FireModeData
{
    [SerializeField] private GameObject damageAreaPrefab;
    [SerializeField] private float distance = 2f;
    [SerializeField] private float stepInterval = 0.5f;

    private Vector3 previousPosition;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 currentPosition = weaponOwner.transform.position;

        // 前回生成位置から一定距離以上移動していたら生成
        if (Vector3.Distance(currentPosition, previousPosition) >= stepInterval)
        {
            CreateDamageArea(weapon, weaponOwner);
            previousPosition = currentPosition;
        }
    }

    private void CreateDamageArea(Weapon weapon, Player weaponOwner)
    {
        Vector3 spawnPos = Camera.main.transform.position - Camera.main.transform.up * distance;

        GameObject area = PoolManager.Instance.Get(damageAreaPrefab);

        area.transform.position = spawnPos;

        area.GetComponent<DamageArea>().Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));
    }
}