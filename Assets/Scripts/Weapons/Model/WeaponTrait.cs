using UnityEngine;

public abstract class WeaponTrait : ScriptableObject
{
    [Tooltip("詳細パネルの「固有効果」に出す説明")]
    [SerializeField, TextArea] private string description;

    public string Description => description;

    public virtual void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods) { }
    public virtual void OnReloadStart(Weapon weapon, ref ReloadStartContext ctx) { }
    public virtual void OnReloadComplete(Weapon weapon) { }
    public virtual void OnReloadCanceled(Weapon weapon) { }
    public virtual void OnFired(Weapon weapon) { }
}
