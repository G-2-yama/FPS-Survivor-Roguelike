using UnityEngine;


public abstract class Item : ScriptableObject
{
    [SerializeField] protected string displayName;
    public string DisplayName => displayName;

    [SerializeField] protected string description;
    public string Description => description;
    
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
}