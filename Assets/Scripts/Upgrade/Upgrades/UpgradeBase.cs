using UnityEngine;


public abstract class UpgradeBase
{
    private string displayName;
    public virtual string DisplayName => displayName;

    private string description;
    public virtual string Description => description;

    public UpgradeBase(string displayName, string description)
    {
        this.displayName = displayName;
        this.description = description;
    }
        
    

    /// <summary>
    /// このアップグレードを対象のゲームオブジェクトに適用します
    /// </summary>
    public abstract void Apply();
}