using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class Item : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("アイテムの ID。同じアイテムかどうかの判定に使う")]
    [SerializeField] protected string itemId;
    [SerializeField] protected string displayName;
    public string DisplayName => displayName;
    [SerializeField] protected Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] protected string description;
    public string Description => description;

    [Header("Levels")]
    [Tooltip("レベル別に足したい文。数値の変化は自動で出るので、ここには書かない")]
    [SerializeField] protected string[] levelDescriptions = new string[0];

    [Header("Attributes")]
    [Tooltip("このアイテムが持つ属性")]
    [SerializeField] protected List<AttributeDefinition> attributes = new List<AttributeDefinition>();
    [Tooltip("すべての基盤に1つずつ数える（シンクロ用）")]
    [SerializeField] protected bool countsAsAllBases;

    [Header("Acquisition")]
    [Tooltip("同じグループのアイテムは1つしか持てない。空なら制限なし")]
    [SerializeField] protected ItemGroup exclusiveGroup;

    public string ItemId => itemId;
    public IReadOnlyList<AttributeDefinition> Attributes => attributes;
    public bool CountsAsAllBases => countsAsAllBases;
    public ItemGroup ExclusiveGroup => exclusiveGroup;

    public abstract int MaxLevel { get; }

    /// <summary>
    /// このアイテムを対象のゲームオブジェクトに適用します
    /// </summary>
    public abstract void Apply(Player player, int level);

    /// <summary>
    /// アイテムの効果を取り消します
    /// </summary>
    public abstract void Revert(Player player, int level);

    public abstract IReadOnlyList<LevelChange> GetLevelChanges(int level);

    public string GetLevelUpDescription(int level)
    {
        IEnumerable<string> lines = GetLevelChanges(level).Select(c => c.ToString());
        int next = level + 1;
        if (levelDescriptions != null && next >= 0 && next < levelDescriptions.Length
            && !string.IsNullOrEmpty(levelDescriptions[next]))
        {
            lines = lines.Append(levelDescriptions[next]);
        }
        return string.Join("\n", lines);
    }

    protected static T ValueAt<T>(T[] values, int level)
    {
        if (values == null || values.Length == 0)
            return default;
        return values[Mathf.Clamp(level, 0, values.Length - 1)];
    }
}
