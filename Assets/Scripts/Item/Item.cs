using UnityEngine;


public abstract class Item : ScriptableObject
{
    [SerializeField] protected string displayName;
    public string DisplayName => displayName;
    [SerializeField] protected Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] protected string description;
    public string Description => description;

    [SerializeField] protected Item nextLevelItem;
    public Item NextLevelItem => nextLevelItem;
    
    protected Player player;

    public void Initialize(Player player)
    {
        this.player = player;
    }

    public virtual bool IsAvailable() => true;
        
    /// <summary>
    /// このアイテムを対象のゲームオブジェクトに適用します
    /// </summary>
    public abstract void Apply();

    
    /// <summary>
    /// アイテムの効果を取り消します
    /// </summary>
    public virtual void Revert() { }
}