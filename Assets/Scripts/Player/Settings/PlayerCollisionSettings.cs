using UnityEngine;

/// <summary>プレイヤー本体の寸法と敵との接触および押し出しの設定を保持する</summary>
[CreateAssetMenu(fileName = "PlayerCollisionSettings", menuName = "Player/設定/衝突判定")]
public sealed class PlayerCollisionSettings : PlayerSettingsAsset
{
    [Header("Body Dimensions")]
    [SerializeField, Min(0.01f), PlayerSettingLabel("Radius"), Tooltip("CharacterControllerの半径 単位はローカル座標のm")]
    private float radius = 0.5f;

    /// <summary>CharacterControllerの半径 単位はローカル座標のm</summary>
    public float Radius => radius;

    [SerializeField, Min(0.02f), PlayerSettingLabel("Height"), Tooltip("CharacterControllerの高さ 半径の2倍以上が必要 単位はm")]
    private float height = 2f;

    /// <summary>CharacterControllerの高さ 半径の2倍以上が必要 単位はm</summary>
    public float Height => height;

    [SerializeField, PlayerSettingLabel("Center"), Tooltip("CharacterController中心のローカル座標")]
    private Vector3 center = Vector3.zero;

    /// <summary>CharacterController中心のローカル座標</summary>
    public Vector3 Center => center;

    [SerializeField, Range(0.1f, 1f), PlayerSettingLabel("Slide Height Multiplier"), Tooltip("通常の本体高さに掛ける倍率 半径の2倍より小さい高さにはならない")]
    private float slideHeightMultiplier = 0.5f;

    /// <summary>スライド中に適用する本体高さの倍率</summary>
    public float SlideHeightMultiplier => slideHeightMultiplier;

    [Header("Movement Collision")]
    [SerializeField, Range(0f, 90f), PlayerSettingLabel("Slope Limit"), Tooltip("CharacterControllerが登れる斜面の最大角度 単位は度")]
    private float slopeLimit = 45f;

    /// <summary>CharacterControllerが登れる斜面の最大角度 単位は度</summary>
    public float SlopeLimit => slopeLimit;

    [SerializeField, Min(0f), PlayerSettingLabel("Step Offset"), Tooltip("CharacterControllerが乗り越える段差の高さ 単位はm")]
    private float stepOffset = 0.3f;

    /// <summary>CharacterControllerが乗り越える段差の高さ 単位はm</summary>
    public float StepOffset => stepOffset;

    [SerializeField, Min(0.001f), PlayerSettingLabel("Skin Width"), Tooltip("衝突時に許容する重なり幅 小さすぎると引っ掛かりやすくなる 単位はm")]
    private float skinWidth = 0.08f;

    /// <summary>衝突時に許容する重なり幅 小さすぎると引っ掛かりやすくなる 単位はm</summary>
    public float SkinWidth => skinWidth;

    [SerializeField, Min(0f), PlayerSettingLabel("Min Move Distance"), Tooltip("この距離未満のCharacterController移動を無視する 単位はm")]
    private float minMoveDistance = 0.001f;

    /// <summary>この距離未満のCharacterController移動を無視する 単位はm</summary>
    public float MinMoveDistance => minMoveDistance;

    [SerializeField, Range(0f, 1f), PlayerSettingLabel("Recoverable Ground Normal Y"), Tooltip("接触面の法線Yがこの値を超えた場合にジャンプ回数を回復する")]
    private float recoverableGroundNormalY = 0.1f;

    /// <summary>接触面の法線Yがこの値を超えた場合にジャンプ回数を回復する</summary>
    public float RecoverableGroundNormalY => recoverableGroundNormalY;

    [Header("Enemy Contact")]
    [SerializeField, PlayerSettingLabel("Enemy Layers"), Tooltip("本体衝突から除外し、接触判定で検知する敵のレイヤー")]
    private LayerMask enemyLayers = 1 << 6;

    /// <summary>本体衝突から除外し、接触判定で検知する敵のレイヤー</summary>
    public LayerMask EnemyLayers => enemyLayers;

    [SerializeField, Range(0f, 31f), PlayerSettingLabel("Body Layer", true), Tooltip("プレイヤー本体に割り当てるレイヤー")]
    private int bodyLayer = 8;

    /// <summary>プレイヤー本体に割り当てるレイヤー</summary>
    public int BodyLayer => bodyLayer;

    [SerializeField, Range(0f, 31f), PlayerSettingLabel("Push Zone Layer", true), Tooltip("敵の接触と押し出しを検知する子オブジェクトのレイヤー")]
    private int pushZoneLayer = 9;

    /// <summary>敵の接触と押し出しを検知する子オブジェクトのレイヤー</summary>
    public int PushZoneLayer => pushZoneLayer;

    [Header("Push")]
    [SerializeField, PlayerSettingLabel("Push Enabled"), Tooltip("敵を水平方向へ押し出す 接触ダメージの有効化とは独立する")]
    private bool pushEnabled = true;

    /// <summary>敵を水平方向へ押し出す 接触ダメージの有効化とは独立する</summary>
    public bool PushEnabled => pushEnabled;

    [SerializeField, Min(0f), PlayerSettingLabel("Push Radius Padding"), Tooltip("本体半径に追加する敵との間隔 単位はm")]
    private float pushRadiusPadding = 0f;

    /// <summary>本体半径に追加する敵との間隔 単位はm</summary>
    public float PushRadiusPadding => pushRadiusPadding;

    [Header("Contact Damage")]
    [SerializeField, PlayerSettingLabel("Contact Damage Enabled"), Tooltip("敵の最初のColliderが入ったときに一度だけダメージを受ける")]
    private bool contactDamageEnabled = true;

    /// <summary>敵の最初のColliderが入ったときに一度だけダメージを受ける</summary>
    public bool ContactDamageEnabled => contactDamageEnabled;

    /// <summary>衝突判定の数値と関連する条件を検証する</summary>
    public override bool Validate(out string error)
    {
        if (!IsFinite(slideHeightMultiplier) || slideHeightMultiplier < 0.1f || slideHeightMultiplier > 1f)
        { error = "スライド本体高さ倍率は0.1から1の範囲にしてください"; return false; }
        if (!IsFinite(radius) || radius < 0.01f) { error = "本体半径の値が有効範囲外です"; return false; }
        if (!IsFinite(height) || height < 0.02f) { error = "本体高さの値が有効範囲外です"; return false; }
        if (!IsFinite(center.x) || !IsFinite(center.y) || !IsFinite(center.z)) { error = "本体中心には有限の数値を指定してください"; return false; }
        if (!IsFinite(slopeLimit) || slopeLimit < 0f || slopeLimit > 90f) { error = "登れる斜面の角度の値が有効範囲外です"; return false; }
        if (!IsFinite(stepOffset) || stepOffset < 0f) { error = "登れる段差の値が有効範囲外です"; return false; }
        if (!IsFinite(skinWidth) || skinWidth < 0.001f) { error = "接触余裕幅の値が有効範囲外です"; return false; }
        if (!IsFinite(minMoveDistance) || minMoveDistance < 0f) { error = "最小移動距離の値が有効範囲外です"; return false; }
        if (!IsFinite(recoverableGroundNormalY) || recoverableGroundNormalY < 0f || recoverableGroundNormalY > 1f) { error = "ジャンプ回復の接触条件の値が有効範囲外です"; return false; }
        if (bodyLayer < 0 || bodyLayer > 31) { error = "本体レイヤーの値が有効範囲外です"; return false; }
        if (pushZoneLayer < 0 || pushZoneLayer > 31) { error = "接触領域レイヤーの値が有効範囲外です"; return false; }
        if (!IsFinite(pushRadiusPadding) || pushRadiusPadding < 0f) { error = "押し出し追加距離の値が有効範囲外です"; return false; }
        if (height < radius * 2f || stepOffset > height) { error = "高さは半径の2倍以上、段差は高さ以下にしてください"; return false; }
        if (enemyLayers.value == 0 || (enemyLayers.value & (1 << bodyLayer | 1 << pushZoneLayer)) != 0 || bodyLayer == pushZoneLayer) { error = "敵、本体、接触領域のレイヤーを重複しないように設定してください"; return false; }
        error = null;
        return true;
    }
}

