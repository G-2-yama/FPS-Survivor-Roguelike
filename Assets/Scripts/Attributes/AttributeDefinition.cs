using System.Collections.Generic;
using UnityEngine;

public enum AttributeKind
{
    Base = 0,
    Module = 1,
}

[CreateAssetMenu(fileName = "Attribute", menuName = "Attributes/Attribute")]
public class AttributeDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("選択画面などに出す名前")]
    [SerializeField] private string displayName;
    [Tooltip("基盤かモジュールか")]
    [SerializeField] private AttributeKind kind = AttributeKind.Base;
    [Tooltip("属性の効果の説明")]
    [SerializeField, TextArea] private string description;

    [Header("Thresholds")]
    [Tooltip("この数以上で有効化する")]
    [SerializeField, Min(1)] private int activateThreshold = 4;
    [Tooltip("この数以上で最大になる。基盤だけが使う")]
    [SerializeField, Min(1)] private int maxThreshold = 7;

    [Header("Effects")]
    [Tooltip("有効化している間に働く効果")]
    [SerializeField] private List<AttributeEffectDefinition> activeEffects = new List<AttributeEffectDefinition>();
    [Tooltip("最大の間に働く効果")]
    [SerializeField] private List<AttributeEffectDefinition> maxEffects = new List<AttributeEffectDefinition>();

    public string DisplayName => displayName;
    public AttributeKind Kind => kind;
    public string Description => description;
    public int ActivateThreshold => activateThreshold;
    public int MaxThreshold => maxThreshold;
    public IReadOnlyList<AttributeEffectDefinition> ActiveEffects => activeEffects;
    public IReadOnlyList<AttributeEffectDefinition> MaxEffects => maxEffects;

    public bool IsActiveAt(int count) => count >= activateThreshold;

    public bool IsMaxAt(int count) => kind == AttributeKind.Base && count >= maxThreshold;
}
