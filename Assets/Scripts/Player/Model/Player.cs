using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;

    [SerializeField] private PlayerInventory inventory;
    public PlayerInventory Inventory => inventory;

    public TeamType TeamType => TeamType.Player;

    public event Action OnDeath;

    [SerializeField] public Weapon leftWeapon;
    public Weapon LeftWeapon => leftWeapon;
    public bool HasLeftWeapon => leftWeapon.WeaponData != null;

    [SerializeField] public Weapon rightWeapon;
    public Weapon RightWeapon => rightWeapon;
    public bool HasRightWeapon => rightWeapon.WeaponData != null;

    [SerializeField] private Weapon leftAbility;
    public Weapon LeftAbility => leftAbility;
    public bool HasLeftAbility => leftAbility.WeaponData != null;

    [SerializeField] private Weapon rightAbility;
    public Weapon RightAbility => rightAbility;
    public bool HasRightAbility => rightAbility.WeaponData != null; 

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
    public void EquipLeftWeapon(WeaponData weapon) => leftWeapon.Equip(weapon);

    /// <summary>
    /// 右手の装備武器を変更する
    /// </summary>
    /// <param name="weapon">装備する武器</param>
    public void EquipRightWeapon(WeaponData weapon) => rightWeapon.Equip(weapon);
    
    public void EquipLeftAbility(WeaponData ability) => leftAbility.Equip(ability);

    public void EquipRightAbility(WeaponData ability) => rightAbility.Equip(ability);

    /// <summary>
    /// 死亡したときに呼び出される処理
    /// </summary>
    private void HandleDeath()
    {
        OnDeath?.Invoke();
    }
}
