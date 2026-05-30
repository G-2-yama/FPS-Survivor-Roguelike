using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/LevelUp")]
public class LevelUp : UpgradeBase
{
    [SerializeField] private SlotType targetType;
    
   

    public override bool IsAvailable()
    {
        Weapon target = player.Inventory.WeaponSlots[targetType];
        UpdateDesription(target.WeaponData.NextLevelData);
        return player.Inventory.HasWeapon(targetType) && target.WeaponData.NextLevelData != null;
    }

    public override void Apply()
    {
        Weapon target = player.Inventory.WeaponSlots[targetType];
        target.LevelUp();
    }

    private void UpdateDesription(WeaponData nextData)
    {
        if (nextData == null)
        {
            return;
        }
        description = nextData.DisplayName + "のレベルが上がる" + "\n" + nextData.Description;
    }
}
