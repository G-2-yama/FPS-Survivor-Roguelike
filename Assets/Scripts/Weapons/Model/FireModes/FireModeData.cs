using UnityEngine;
/// <summary>武器の攻撃方式と共通のダメージ補正を提供する</summary>

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
    /// <summary>命中位置へ設定されたエフェクトを生成する</summary>

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
        return weapon.CurrentLevel.DamageProfile.GetDamageAmount(weaponOwner);
    }
    /// <summary>所有者の強化を反映したノックバック量を計算する</summary>

    protected float GetKnockbackForce(Weapon weapon, Player weaponOwner)
    {
        return weapon.CurrentLevel.DamageProfile.GetKnockbackForce(weaponOwner);
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

        float spread = weapon.CurrentLevel.SpreadAngle * 0.5f;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 direction = weapon.AimTransform.forward;
        direction = Quaternion.Euler(y, x, 0) * direction;

        return direction;
    }
}
