using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/GetItem")]
public class GetItem : UpgradeBase
{
    [SerializeField] private Item target;

    public override bool IsAvailable()
    {
        foreach (var item in player.Inventory.Items)
        {
            if (item == null) return true;
        }
        return false;
    }

    public override void Apply()
    {
        player.Inventory.EquipItem(target);
    }
}