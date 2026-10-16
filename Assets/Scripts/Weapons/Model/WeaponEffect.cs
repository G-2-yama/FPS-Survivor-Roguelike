public abstract class WeaponEffect
{
    public virtual void OnAdded(Player player) { }
    public virtual void OnRemoved(Player player) { }
    public virtual void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods) { }
    public virtual void OnReloadStart(Weapon weapon, ref ReloadStartContext ctx) { }
    public virtual void OnReloadComplete(Weapon weapon) { }
    public virtual void OnReloadCanceled(Weapon weapon) { }
    public virtual void OnFired(Weapon weapon) { }
    public virtual bool NegatesDamage(Player player) => false;
}

public struct ReloadStartContext
{
    public bool LockMovement;
}
