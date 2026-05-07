using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// プレイヤーの装備と体力をまとめて保持するモデル。
/// 移動や入力の実処理は別クラスへ寄せ、ここでは他システムが参照する状態のみを管理する。
/// </summary>
public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;

    [SerializeField] private PlayerInventory inventory;
    public PlayerInventory Inventory => inventory;

    public TeamType TeamType => TeamType.Player;

    public event Action OnDeath;
    public event Action OnLevelUp;

    [SerializeField] private Weapon leftWeapon;
    public Weapon LeftWeapon => leftWeapon;
    public bool HasLeftWeapon => leftWeapon != null && leftWeapon.WeaponData != null;

    [SerializeField] private Weapon rightWeapon;
    public Weapon RightWeapon => rightWeapon;
    public bool HasRightWeapon => rightWeapon != null && rightWeapon.WeaponData != null;

    [SerializeField] private Weapon leftAbility;
    public Weapon LeftAbility => leftAbility;
    public bool HasLeftAbility => leftAbility != null && leftAbility.WeaponData != null;

    [SerializeField] private Weapon rightAbility;
    public Weapon RightAbility => rightAbility;
    public bool HasRightAbility => rightAbility != null && rightAbility.WeaponData != null;

    [SerializeField] private Weapon rightAutoWeapon;
    public Weapon RightAutoWeapon => rightAutoWeapon;
    public bool HasRightAutoWeapon => rightAutoWeapon != null && rightAutoWeapon.WeaponData != null;

    [SerializeField] private Weapon leftAutoWeapon;
    public Weapon LeftAutoWeapon => leftAutoWeapon;
    public bool HasLeftAutoWeapon => leftAutoWeapon != null && leftAutoWeapon.WeaponData != null;

    List<Item> items = new List<Item>();
    public IReadOnlyList<Item> Items => items;

    /// <summary>
    /// プレイヤーの体力モデル
    /// </summary>
    public Health Health { get; private set; }
    
    private int level = 1;
    public int Level => level;

    private void Awake()
    {
        int initialHp = config != null ? config.InitialHP : 100;
        Health = new Health(initialHp);
        Health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnDeath -= HandleDeath;
        }
    }

    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    public void LevelUp()
    {
        level++;
        OnLevelUp?.Invoke();
    }

    public void EquipLeftWeapon(WeaponData weapon) => leftWeapon?.Equip(weapon);

    public void EquipRightWeapon(WeaponData weapon) => rightWeapon?.Equip(weapon);

    public void EquipLeftAbility(WeaponData ability) => leftAbility?.Equip(ability);

    public void EquipRightAbility(WeaponData ability) => rightAbility?.Equip(ability);

    public void EquipLeftAutoWeapon(WeaponData autoWeapon) => leftAutoWeapon?.Equip(autoWeapon);

    public void EquipRightAutoWeapon(WeaponData autoWeapon) => rightAutoWeapon?.Equip(autoWeapon);

    public void EquiptItem(Item item)
    {
        items.Add(item);
        item.Initialize(this);
        item.Apply();
    }
    private void HandleDeath()
    {
        OnDeath?.Invoke();
    }
}
