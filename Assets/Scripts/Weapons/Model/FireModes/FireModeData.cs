using Unity.VisualScripting;
using UnityEngine;

public abstract class FireModeData : ScriptableObject
{
    /// <summary>
    /// 攻撃処理を実装するメソッド
    /// </summary>
    /// <param name="weapon">攻撃を行う武器</param>
    /// <param name="direction">攻撃方向</param>
    public abstract void Fire(Weapon weapon, Vector3 direction);

    /// <summary>
    /// 当たったコライダーからダメージ対象を取得し、必要ならダメージを適用する
    /// </summary>
    /// <returns>ダメージを与えた場合はtrue</returns>
    public bool TryApplyDamage(Weapon weapon, Collider hitCollider)
    {
        var damageable = hitCollider.GetComponentInParent<IDamageable>();
        if (damageable == null) return false;
        
        if (damageable.TeamType != TeamType.Enemy 
            && damageable.TeamType != TeamType.Boss) return false;

        damageable.TakeDamage(GetDamageAmount(weapon));
        return true;
    }

    /// <summary>
    /// 武器の攻撃力を整数ダメージとして取得する
    /// </summary>
    protected int GetDamageAmount(Weapon weapon)
    {
        return Mathf.RoundToInt(weapon.WeaponData.Damage);
    }
}
