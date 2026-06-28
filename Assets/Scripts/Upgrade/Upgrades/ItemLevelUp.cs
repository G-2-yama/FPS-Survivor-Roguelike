using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/ItemLevelUp")]
public class ItemLevelUp : UpgradeBase
{
    [SerializeField] private int itemIndex;

    public override bool IsAvailable()
    {
        UpdateDesription(player.Inventory.Items[itemIndex]?.NextLevelItem);
        return player.Inventory.Items[itemIndex]?.NextLevelItem != null;
    }

    public override void Apply()
    {
        Item targetItem = player.Inventory.Items[itemIndex];
        player.Inventory.DiscardItem(itemIndex);
        player.Inventory.EquipItem(targetItem.NextLevelItem);
    }

    private void UpdateDesription(Item nextItem)
    {
        if (nextItem == null)
        {
            return;
        }
        icon = nextItem.Icon;
        displayName = nextItem.DisplayName + " 強化";
        description = nextItem.Description;
    }
}
