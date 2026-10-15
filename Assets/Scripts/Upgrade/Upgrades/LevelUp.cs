using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/LevelUp")]
public class LevelUp : UpgradeBase
{
    [SerializeField] private SlotType targetType;
    public override bool IsAvailable()
    {
        Weapon weapon = player.Inventory.GetWeapon(targetType);
        if (weapon == null || !weapon.CanLevelUp)
            return false;

        UpdateDescription(weapon);
        return true;
    }

    public override void Apply()
    {
        player.Inventory.LevelUpWeapon(targetType);
    }

    /// <summary>
    /// 武器のレベルアップの説明更新
    /// </summary>
    private void UpdateDescription(Weapon weapon)
    {
        WeaponLevelData next = weapon.WeaponData.GetLevel(weapon.Level + 1);
        icon = next.Icon;
        displayName = next.DisplayName + " 強化";
        LevelStep = new UpgradeLevelStep(weapon.Level + 1, weapon.Level + 2, weapon.WeaponData.IsEvoLevel(weapon.Level + 1));

        IReadOnlyList<LevelChange> changes = weapon.WeaponData.GetLevelChanges(weapon.Level);
        description = changes.Count > 0 ? string.Join("\n", changes.Select(c => c.ToString())) : "基礎ステータスの変化なし";
    }
}
