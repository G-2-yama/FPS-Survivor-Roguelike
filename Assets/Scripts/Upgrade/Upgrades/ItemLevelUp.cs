using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/ItemLevelUp")]
public class ItemLevelUp : UpgradeBase
{
    [SerializeField] private int itemIndex;

    public override bool IsAvailable()
    {
        OwnedItem owned = player.Inventory.Items[itemIndex];
        if (owned == null || !owned.CanLevelUp)
            return false;

        UpdateDesription(owned);
        return true;
    }

    public override void Apply()
    {
        player.Inventory.LevelUpItem(itemIndex);
    }

    public override void AddToPreview(AttributePreview preview)
    {
        preview.AddItem(player.Inventory.Items[itemIndex]?.Item);
    }

    private void UpdateDesription(OwnedItem owned)
    {
        icon = owned.Item.Icon;
        displayName = owned.Item.DisplayName + " 強化";
        LevelStep = new UpgradeLevelStep(owned.Level + 1, owned.Level + 2, false);
        description = owned.Item.GetLevelUpDescription(owned.Level);
    }
}
