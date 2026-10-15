using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Group")]
public class ItemGroup : ScriptableObject
{
    [Tooltip("グループの表示名")]
    [SerializeField] private string displayName;
    public string DisplayName => displayName;
}
