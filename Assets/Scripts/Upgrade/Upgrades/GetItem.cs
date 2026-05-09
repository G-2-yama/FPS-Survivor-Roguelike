using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/GetItem")]
public class GetItem : UpgradeBase
{
    [SerializeField] private Item target;

    public override bool IsAvailable()
    {
        return player.Inventory.Items.Count < 6;
    }

    public override void Apply()
    {
        player.Inventory.EquipItem(target);
    }
}