using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/GetItem")]
public class GetItem : UpgradeBase
{
    [SerializeField] private Item target;

    public override bool IsAvailable()
    {
        UpdateDescription();
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

    /// <summary>
    /// アイテム取得の説明更新
    /// </summary>
    private void UpdateDescription()
    {
        if (target == null)
        {
            return;
        }

        icon = target.Icon;
        displayName = target.DisplayName + " 解放";
        description = target.Description;
    }
}