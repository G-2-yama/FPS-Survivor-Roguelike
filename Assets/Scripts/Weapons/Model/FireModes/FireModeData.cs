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

    public void TryEnableHitEffect(Vector3 position)
    {
        if (effectPrefab == null)
        {
            return;
        }

        var effectInstance = PoolManager.Instance.Get(effectPrefab);
        effectInstance.transform.position = position;
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
