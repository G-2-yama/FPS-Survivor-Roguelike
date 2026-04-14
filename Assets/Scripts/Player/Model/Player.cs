using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;

    public TeamType TeamType => TeamType.Player;

    public event Action OnDeath;

    [SerializeField] public Weapon leftWeapon;
    public Weapon LeftWeapon => leftWeapon;
    public bool HasLeftWeapon => leftWeapon != null && leftWeapon.WeaponData != null;

    [SerializeField] public Weapon rightWeapon;
    public Weapon RightWeapon => rightWeapon;
    public bool HasRightWeapon => rightWeapon != null && rightWeapon.WeaponData != null;

    /// <summary>
    /// プレイヤーの体力を管理するモデル
    /// </summary>
    public Health Health { get; private set; }

    private void Awake()
    {
        Health = new Health(config.InitialHP);
        Health.OnDeath += HandleDeath;
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    /// <summary>
    /// 左手の装備武器を変更する
    /// </summary>
    /// <param name="weapon">装備する武器</param>
    public void EquipLeftWeapon(WeaponData weapon)
    {
        if (leftWeapon == null)
        {
            return;
        }

        leftWeapon.Equip(weapon);
    }

    /// <summary>
    /// 右手の装備武器を変更する
    /// </summary>
    /// <param name="weapon">装備する武器</param>
    public void EquipRightWeapon(WeaponData weapon)
    {
        if (rightWeapon == null)
        {
            return;
        }

        rightWeapon.Equip(weapon);
    }

    /// <summary>
    /// 左右の武器の装備状態を入れ替える
    /// </summary>
    /// <returns>入れ替えに成功した場合はtrue</returns>
    public bool SwapWeapons()
    {
        if (!HasLeftWeapon && !HasRightWeapon)
        {
            return false;
        }

        leftWeapon.SwapLoadoutWith(rightWeapon);
        return true;
    }

    /// <summary>
    /// 死亡したときに呼び出される処理
    /// </summary>
    private void HandleDeath()
    {
        OnDeath?.Invoke();
    }
}
