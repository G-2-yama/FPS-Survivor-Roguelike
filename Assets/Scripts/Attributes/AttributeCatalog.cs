using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttributeCatalog", menuName = "Attributes/Attribute Catalog")]
public class AttributeCatalog : ScriptableObject
{
    [Tooltip("ゲームで使うすべての属性")]
    [SerializeField] private List<AttributeDefinition> all = new List<AttributeDefinition>();

    public IReadOnlyList<AttributeDefinition> All => all;
}
