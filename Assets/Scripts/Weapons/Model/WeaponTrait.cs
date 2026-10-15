using UnityEngine;

public abstract class WeaponTrait : ScriptableObject
{
    public virtual void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods) { }
    public virtual void OnReloadStart(Weapon weapon, ref ReloadStartContext ctx) { }
    public virtual void OnReloadComplete(Weapon weapon) { }
    public virtual void OnReloadCanceled(Weapon weapon) { }
    public virtual void OnFired(Weapon weapon) { }
}
