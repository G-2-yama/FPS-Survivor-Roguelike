using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/ItemLevelUp")]
public class ItemLevelUp : UpgradeBase
{
    [SerializeField] private int itemIndex;

    public override bool IsAvailable()
    {
        return player.Inventory.Items[itemIndex]?.NextLevelItem != null;
    }

    public override void Apply()
    {
        Item targetItem = player.Inventory.Items[itemIndex];
        player.Inventory.DiscardItem(itemIndex);
        player.Inventory.EquipItem(targetItem.NextLevelItem);
    }
}
