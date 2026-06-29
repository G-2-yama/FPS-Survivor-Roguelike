using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public abstract class UpgradeBase : ScriptableObject
{
    [SerializeField] protected string displayName;
    public string DisplayName => displayName;

    [SerializeField] protected string description;
    public string Description => description;
    [SerializeField] protected Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] protected Rarity rarity;
    public Rarity Rarity => rarity;

    [SerializeField][Min(0)] protected int weight = 100;
    public int Weight => weight;

    protected Player player;

    public void Initialize(Player player)
    {
        this.player = player;
    }

    public virtual bool IsAvailable() => true;
        
    /// <summary>
    /// このアップグレードを対象のゲームオブジェクトに適用します
    /// </summary>
    public abstract void Apply();
}