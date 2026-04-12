using UnityEngine;

public class LevelUp : UpgradeBase
{
    private GameObject target;

    public LevelUp(string displayName, string description, GameObject target) : base(displayName, description)
    {
        this.target = target;
    }

    public override void Apply()
    {
        var weapon = target.GetComponent<Weapon>();
        weapon.LevelUp();
        Debug.Log($"{weapon.WeaponData.DisplayName} leveled up to {weapon.Level}!");
    }
}
