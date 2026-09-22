using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public static class RarityExtensions
{
    /// <summary>
    /// レアリティに対応する色を取得する
    /// </summary>
    /// <param name="rarity">色を取得するレアリティ</param>
    /// <param name="alpha">色に設定する透明度</param>
    /// <returns>レアリティに対応する色</returns>
    public static Color GetColor(this Rarity rarity, float alpha = 1f)
    {
        Color color = rarity switch
        {
            Rarity.Uncommon => new Color(0.5f, 1f, 0.5f),
            Rarity.Rare => new Color(0f, 0.5f, 1f),
            Rarity.Epic => new Color(0.6f, 0f, 0.6f),
            Rarity.Legendary => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };

        color.a = Mathf.Clamp01(alpha);
        return color;
    }
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
