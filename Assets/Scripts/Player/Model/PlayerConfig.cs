using UnityEngine;

/// <summary>用途別のプレイヤー設定SOをまとめる入口となるコンポーネント</summary>
[DisallowMultipleComponent]
public sealed class PlayerConfig : MonoBehaviour
{
    [SerializeField, PlayerSettingLabel("ステータス設定"), Tooltip("初期体力、移動能力、攻撃倍率などを設定するSO")]
    private PlayerStatsSettings stats;
    [SerializeField, PlayerSettingLabel("操作設定"), Tooltip("移動、ジャンプ、視点操作などの調整値を設定するSO")]
    private PlayerControlSettings control;
    [SerializeField, PlayerSettingLabel("衝突判定設定"), Tooltip("本体サイズ、敵との接触、押し出しを設定するSO")]
    private PlayerCollisionSettings collision;
    [SerializeField, PlayerSettingLabel("カメラ演出設定"), Tooltip("操作に応じた視点高さと視野角の変化を設定するSO")]
    private PlayerCameraSettings cameraSettings;
    [SerializeField, PlayerSettingLabel("レベル成長設定"), Tooltip("初期レベルと必要経験値の増加を設定するSO")]
    private PlayerProgressionSettings progression;

    /// <summary>初期ステータスの共有設定</summary>
    public PlayerStatsSettings Stats => stats;
    /// <summary>操作方法の共有設定</summary>
    public PlayerControlSettings Control => control;
    /// <summary>本体寸法と接触の共有設定</summary>
    public PlayerCollisionSettings Collision => collision;
    /// <summary>カメラ演出の共有設定</summary>
    public PlayerCameraSettings Camera => cameraSettings;
    /// <summary>レベル成長の共有設定</summary>
    public PlayerProgressionSettings Progression => progression;

    /// <summary>設定の不足と数値の不正を確認する</summary>
    public bool Validate(out string error)
    {
        PlayerSettingsAsset[] settings = { stats, control, collision, cameraSettings, progression };
        string[] labels = { "ステータス", "操作", "衝突判定", "カメラ演出", "レベル成長" };
        for (int i = 0; i < settings.Length; i++)
        {
            if (settings[i] == null) { error = labels[i] + "設定SOが未設定です"; return false; }
            if (!settings[i].Validate(out error)) { error = labels[i] + ": " + error; return false; }
        }
        if (string.IsNullOrEmpty(LayerMask.LayerToName(collision.BodyLayer))
            || string.IsNullOrEmpty(LayerMask.LayerToName(collision.PushZoneLayer)))
        {
            error = "本体と接触領域には登録済みのレイヤーを指定してください";
            return false;
        }
        for (int layer = 0; layer < 32; layer++)
        {
            if ((collision.EnemyLayers.value & (1 << layer)) != 0 && string.IsNullOrEmpty(LayerMask.LayerToName(layer)))
            {
                error = "敵の対象には登録済みのレイヤーを指定してください";
                return false;
            }
        }
        error = null;
        return true;
    }

    /// <summary>開始時の設定を複製し、共有アセットから実行中の状態を分離する</summary>
    public PlayerSettingsSnapshot CreateRuntimeSnapshot() => new PlayerSettingsSnapshot(this);
}
