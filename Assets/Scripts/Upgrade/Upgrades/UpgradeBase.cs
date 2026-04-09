using UnityEngine;


public abstract class UpgradeBase
{
    public virtual string DisplayName => "Unknown Upgrade";
    public virtual string Description => "No description available";

    /// <summary>
    /// このアップグレードを対象のゲームオブジェクトに適用します
    /// </summary>
    /// <param name="target">対象のゲームオブジェクト</param>
    public abstract void Apply(GameObject target);
}