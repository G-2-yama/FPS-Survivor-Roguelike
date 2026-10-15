using UnityEngine;
/// <summary>照準方向へ攻撃個体を発射する</summary>

[CreateAssetMenu(menuName = "Weapons/FireMode/Projectile")]
public class NormalProjectileFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;
    [SerializeField] private Vector3 Offset = new Vector3(0f, 0f, 0.0f);
    [SerializeField] private bool isTracking = false;


    /// <inheritdoc />
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);

        PooledInstance spawned = PoolManager.Instance.Rent(prefab);
        GameObject bullet = spawned.GameObject;

        var damage = spawned.RequireDamage();
        damage.Initialize(weapon.GetDamageAmount(), weapon.GetKnockbackForce());

        MovementBase movement = spawned.Movement;
        movement?.Initialize(weaponOwner.transform, direction);

        Quaternion rotation = Quaternion.LookRotation(direction);
        bullet.transform.rotation = rotation;
        bullet.transform.position =weapon.AimTransform.position + rotation * Offset;
        if(isTracking)
        {
            bullet.transform.SetParent(weapon.AimTransform);
        }
    }
    
}
