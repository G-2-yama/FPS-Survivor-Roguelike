using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttributeCatalog", menuName = "Attributes/Attribute Catalog")]
public class AttributeCatalog : ScriptableObject
{
    [Tooltip("ゲームで使うすべての属性")]
    [SerializeField] private List<AttributeDefinition> all = new List<AttributeDefinition>();

    [Header("Equipment")]
    [Tooltip("詳細パネルの「所属する装備」に並べる武器。区分ごとに、この順で並ぶ")]
    [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
    [Tooltip("詳細パネルの「所属する装備」に並べるアイテム。この順で並ぶ")]
    [SerializeField] private List<Item> items = new List<Item>();

    public IReadOnlyList<AttributeDefinition> All => all;
    public IReadOnlyList<WeaponData> Weapons => weapons;
    public IReadOnlyList<Item> Items => items;
}
