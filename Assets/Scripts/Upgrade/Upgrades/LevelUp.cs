using UnityEngine;

public class LevelUp : UpgradeBase
{
    public override string DisplayName => "レベルアップ";
    public override string Description => "武器のレベルを上げます";

    public override void Apply(GameObject target)
    {
        var weapon = target.GetComponent<Weapon>();
        weapon.LevelUp();
        Debug.Log($"{weapon.WeaponData.DisplayName} leveled up to {weapon.Level}!");
    }
}
