using UnityEngine;

/// <summary>プレイヤーの初期レベルと経験値による成長設定を保持する</summary>
[CreateAssetMenu(fileName = "PlayerProgressionSettings", menuName = "Player/設定/レベル成長")]
public sealed class PlayerProgressionSettings : PlayerSettingsAsset
{
    [Header("Progression")]
    [SerializeField, Min(1f), PlayerSettingLabel("Initial Level"), Tooltip("開始時のプレイヤーレベル")]
    private int initialLevel = 1;

    /// <summary>開始時のプレイヤーレベル</summary>
    public int InitialLevel => initialLevel;

    [SerializeField, Min(1f), PlayerSettingLabel("Level Up Required Exp"), Tooltip("最初のレベルアップに必要な経験値")]
    private float levelUpRequiredExp = 10f;

    /// <summary>最初のレベルアップに必要な経験値</summary>
    public float LevelUpRequiredExp => levelUpRequiredExp;

    [SerializeField, Min(1f), PlayerSettingLabel("Level Up Rate"), Tooltip("レベルアップするたびに次の必要経験値へ掛ける倍率")]
    private float levelUpRate = 1.2f;

    /// <summary>レベルアップするたびに次の必要経験値へ掛ける倍率</summary>
    public float LevelUpRate => levelUpRate;

    /// <summary>レベル成長の数値と関連する条件を検証する</summary>
    public override bool Validate(out string error)
    {
        if (initialLevel < 1) { error = "初期レベルの値が有効範囲外です"; return false; }
        if (!IsFinite(levelUpRequiredExp) || levelUpRequiredExp < 1f) { error = "初回の必要経験値の値が有効範囲外です"; return false; }
        if (!IsFinite(levelUpRate) || levelUpRate < 1f) { error = "必要経験値の増加倍率の値が有効範囲外です"; return false; }
        error = null;
        return true;
    }
}

