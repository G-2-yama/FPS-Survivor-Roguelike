using UnityEngine;

public abstract class FireModeData : ScriptableObject
{
    [SerializeField] private GameObject effectPrefab;
    public GameObject EffectPrefab => effectPrefab;

    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <param name="weapon">攻撃を行う武器</param>
    /// <param name="weaponOwner">武器の所有者</param>
    public abstract void Fire(Weapon weapon, Player weaponOwner);

    /// <summary>
    /// 当たったコライダーからダメージ対象を取得し、必要ならダメージを適用する
    /// </summary>
    /// <returns>ダメージを与えた場合はtrue</returns>
    public bool TryApplyDamage(Weapon weapon, Collider hitCollider, Player weaponOwner)
    {
        var damageable = hitCollider.GetComponent<IDamageable>();
        if (damageable == null) return false;
        if (damageable.TeamType == TeamType.Player) return false;

        damageable.TakeDamage(GetDamageAmount(weapon, weaponOwner));
        return true;
    }

    public bool TryEnableHitEffect(out GameObject effectInstance)
    {
        if (effectPrefab == null)
        {
            effectInstance = null;
            return false;
        }

        effectInstance = PoolManager.Instance.Get(effectPrefab);
        return true;
    }

    /// <summary>
    /// 武器の攻撃力を整数ダメージとして取得する
    /// </summary>
    protected int GetDamageAmount(Weapon weapon, Player weaponOwner)
    {
        return weapon.WeaponData.DamageProfile.GetDamageAmount(weaponOwner);
    }

    /// <summary>
    /// 発射方向をスプレッド角度に基づいてランダムに決定するメソッド
    /// </summary>
    /// <param name="weapon">攻撃を行う武器</param>
    /// <returns>発射方向</returns>
    protected Vector3 GetFireDirection(Weapon weapon)
    {
        if (!weapon.HasWeapon || weapon.WeaponData == null)
        {
            return Vector3.zero;
        }

        float spread = weapon.WeaponData.SpreadAngle * 0.5f;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 direction = Camera.main.transform.forward;
        direction = Quaternion.Euler(y, x, 0) * direction;

        return direction;
    }
}
