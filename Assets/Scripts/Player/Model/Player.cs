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

    [SerializeField] public Weapon rightWeapon;
    public Weapon RightWeapon => rightWeapon;

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
        leftWeapon.Equip(weapon);
    }

    /// <summary>
    /// 右手の装備武器を変更する
    /// </summary>
    /// <param name="weapon">装備する武器</param>
    public void EquipRightWeapon(WeaponData weapon)
    {
        rightWeapon.Equip(weapon);
    }

    /// <summary>
    /// 死亡したときに呼び出される処理
    /// </summary>
    private void HandleDeath()
    {
        OnDeath?.Invoke();
    }
}
