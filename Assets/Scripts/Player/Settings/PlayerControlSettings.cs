using UnityEngine;

/// <summary>移動と視点の操作方法を調整する値を保持する</summary>
[CreateAssetMenu(fileName = "PlayerControlSettings", menuName = "Player/設定/操作")]
public sealed class PlayerControlSettings : PlayerSettingsAsset
{
    [Header("Movement")]
    [SerializeField, Min(0f), PlayerSettingLabel("Ground Deceleration"), Tooltip("地上で移動入力を離したときの減速度 単位はm/秒²")]
    private float groundDeceleration = 30f;

    /// <summary>地上で移動入力を離したときの減速度 単位はm/秒²</summary>
    public float GroundDeceleration => groundDeceleration;

    [SerializeField, PlayerSettingLabel("Gravity"), Tooltip("毎秒加算する垂直加速度 下向きは負の値 単位はm/秒²")]
    private float gravity = -18f;

    /// <summary>毎秒加算する垂直加速度 下向きは負の値 単位はm/秒²</summary>
    public float Gravity => gravity;

    [SerializeField, Min(0.01f), PlayerSettingLabel("Air Reverse To Zero Time"), Tooltip("空中で逆方向へ向いたときの減速と加速の目安時間 単位は秒")]
    private float airReverseToZeroTime = 0.5f;

    /// <summary>空中で逆方向へ向いたときの減速と加速の目安時間 単位は秒</summary>
    public float AirReverseToZeroTime => airReverseToZeroTime;

    [SerializeField, PlayerSettingLabel("Grounded Vertical Velocity"), Tooltip("接地を維持するための垂直速度 負の値で床へ向かう 単位はm/秒")]
    private float groundedVerticalVelocity = -2f;

    /// <summary>接地を維持するための垂直速度 負の値で床へ向かう 単位はm/秒</summary>
    public float GroundedVerticalVelocity => groundedVerticalVelocity;

    [SerializeField, Min(0f), PlayerSettingLabel("Stationary Speed Threshold"), Tooltip("接地中の水平速度がこれ未満なら立ち止まっているとみなす 単位はm/秒")]
    private float stationarySpeedThreshold = 0.1f;

    public float StationarySpeedThreshold => stationarySpeedThreshold;

    [Header("Jump")]
    [SerializeField, Min(1f), PlayerSettingLabel("Max Jump Count"), Tooltip("接地で回復するジャンプ回数 2なら二段ジャンプ")]
    private int maxJumpCount = 2;

    /// <summary>接地で回復するジャンプ回数 2なら二段ジャンプ</summary>
    public int MaxJumpCount => maxJumpCount;

    [SerializeField, Min(0f), PlayerSettingLabel("Jump Hold Duration"), Tooltip("追加の上向き速度を与える時間 単位は秒 0で長押し補正を無効にする")]
    private float jumpHoldDuration = 0.2f;

    /// <summary>追加の上向き速度を与える時間 単位は秒 0で長押し補正を無効にする</summary>
    public float JumpHoldDuration => jumpHoldDuration;

    [SerializeField, Min(1f), PlayerSettingLabel("Jump Hold Max Multiplier"), Tooltip("ジャンプ初速に対する長押し後の最大倍率 1で補正なし")]
    private float jumpHoldMaxMultiplier = 1.2f;

    /// <summary>ジャンプ初速に対する長押し後の最大倍率 1で補正なし</summary>
    public float JumpHoldMaxMultiplier => jumpHoldMaxMultiplier;

    [Header("Look")]
    [SerializeField, Min(0f), PlayerSettingLabel("Look Sensitivity"), Tooltip("マウス移動量に掛ける回転角の係数")]
    private float lookSensitivity = 0.08f;

    /// <summary>マウス移動量に掛ける回転角の係数</summary>
    public float LookSensitivity => lookSensitivity;

    [SerializeField, Min(0f), PlayerSettingLabel("Gamepad Look Sensitivity"), Tooltip("右スティック最大入力時の旋回速度 単位は度/秒")]
    private float gamepadLookSensitivity = 60f;

    /// <summary>右スティック最大入力時の旋回速度 単位は度/秒</summary>
    public float GamepadLookSensitivity => gamepadLookSensitivity;

    [SerializeField, Range(0f, 0.95f), PlayerSettingLabel("Gamepad Look Deadzone"), Tooltip("中心付近の入力を無視する範囲 0から0.95まで")]
    private float gamepadLookDeadzone = 0.25f;

    /// <summary>中心付近の入力を無視する範囲 0から0.95まで</summary>
    public float GamepadLookDeadzone => gamepadLookDeadzone;

    [SerializeField, Range(-89f, 89f), PlayerSettingLabel("Min Pitch"), Tooltip("上方向へ向ける角度の下限 単位は度")]
    private float minPitch = -80f;

    /// <summary>上方向へ向ける角度の下限 単位は度</summary>
    public float MinPitch => minPitch;

    [SerializeField, Range(-89f, 89f), PlayerSettingLabel("Max Pitch"), Tooltip("下方向へ向ける角度の上限 単位は度")]
    private float maxPitch = 80f;

    /// <summary>下方向へ向ける角度の上限 単位は度</summary>
    public float MaxPitch => maxPitch;

    [Header("Slide")]
    [SerializeField, Min(0f), PlayerSettingLabel("Slide Speed"), Tooltip("スライド中の移動速度 単位はm/秒")]
    private float slideSpeed = 12f;

    /// <summary>スライド中の移動速度 単位はm/秒</summary>
    public float SlideSpeed => slideSpeed;

    [SerializeField, Min(0f), PlayerSettingLabel("Slide Min Inertia Speed"), Tooltip("スライドへ移行するために必要な水平速度 単位はm/秒")]
    private float slideMinInertiaSpeed = 1.5f;

    /// <summary>スライドへ移行するために必要な水平速度 単位はm/秒</summary>
    public float SlideMinInertiaSpeed => slideMinInertiaSpeed;

    [SerializeField, Min(0f), PlayerSettingLabel("Slide Turn Rate Degrees Per Second"), Tooltip("スライド中に向きを変えられる角速度 単位は度/秒")]
    private float slideTurnRateDegreesPerSecond = 180f;

    /// <summary>スライド中に向きを変えられる角速度 単位は度/秒</summary>
    public float SlideTurnRateDegreesPerSecond => slideTurnRateDegreesPerSecond;

    [Header("Fast Fall")]
    [SerializeField, Min(0f), PlayerSettingLabel("Fast Fall Entry Speed"), Tooltip("急降下に入った瞬間の下向き速度の大きさ 単位はm/秒")]
    private float fastFallEntrySpeed = 20f;

    /// <summary>急降下に入った瞬間の下向き速度の大きさ 単位はm/秒</summary>
    public float FastFallEntrySpeed => fastFallEntrySpeed;

    [SerializeField, Min(0f), PlayerSettingLabel("Fast Fall Acceleration"), Tooltip("急降下中の追加の下向き加速度 単位はm/秒²")]
    private float fastFallAcceleration = 90f;

    /// <summary>急降下中の追加の下向き加速度 単位はm/秒²</summary>
    public float FastFallAcceleration => fastFallAcceleration;

    [SerializeField, Min(0f), PlayerSettingLabel("Fast Fall Terminal Speed"), Tooltip("急降下更新時の下向き速度の上限 単位はm/秒")]
    private float fastFallTerminalSpeed = 35f;

    /// <summary>急降下更新時の下向き速度の上限 単位はm/秒</summary>
    public float FastFallTerminalSpeed => fastFallTerminalSpeed;

    /// <summary>操作の数値と関連する条件を検証する</summary>
    public override bool Validate(out string error)
    {
        if (!IsFinite(groundDeceleration) || groundDeceleration < 0f) { error = "地上減速の値が有効範囲外です"; return false; }
        if (!IsFinite(gravity) || gravity > 0f) { error = "重力の値が有効範囲外です"; return false; }
        if (!IsFinite(airReverseToZeroTime) || airReverseToZeroTime < 0.01f) { error = "空中切り返し時間の値が有効範囲外です"; return false; }
        if (!IsFinite(groundedVerticalVelocity) || groundedVerticalVelocity > 0f) { error = "接地中の下向き速度の値が有効範囲外です"; return false; }
        if (!IsFinite(stationarySpeedThreshold) || stationarySpeedThreshold < 0f) { error = "立ち止まりとみなす速度の値が有効範囲外です"; return false; }
        if (maxJumpCount < 1) { error = "最大ジャンプ回数の値が有効範囲外です"; return false; }
        if (!IsFinite(jumpHoldDuration) || jumpHoldDuration < 0f) { error = "長押し受付時間の値が有効範囲外です"; return false; }
        if (!IsFinite(jumpHoldMaxMultiplier) || jumpHoldMaxMultiplier < 1f) { error = "長押し最大倍率の値が有効範囲外です"; return false; }
        if (!IsFinite(lookSensitivity) || lookSensitivity < 0f) { error = "マウス視点感度の値が有効範囲外です"; return false; }
        if (!IsFinite(gamepadLookSensitivity) || gamepadLookSensitivity < 0f) { error = "ゲームパッド旋回速度の値が有効範囲外です"; return false; }
        if (!IsFinite(gamepadLookDeadzone) || gamepadLookDeadzone < 0f || gamepadLookDeadzone > 0.95f) { error = "右スティック無効範囲の値が有効範囲外です"; return false; }
        if (!IsFinite(minPitch) || minPitch < -89f || minPitch > 89f) { error = "上下視点の最小角度の値が有効範囲外です"; return false; }
        if (!IsFinite(maxPitch) || maxPitch < -89f || maxPitch > 89f) { error = "上下視点の最大角度の値が有効範囲外です"; return false; }
        if (!IsFinite(slideSpeed) || slideSpeed < 0f) { error = "スライド速度の値が有効範囲外です"; return false; }
        if (!IsFinite(slideMinInertiaSpeed) || slideMinInertiaSpeed < 0f) { error = "スライド継続の最低速度の値が有効範囲外です"; return false; }
        if (!IsFinite(slideTurnRateDegreesPerSecond) || slideTurnRateDegreesPerSecond < 0f) { error = "スライド旋回速度の値が有効範囲外です"; return false; }
        if (!IsFinite(fastFallEntrySpeed) || fastFallEntrySpeed < 0f) { error = "急降下開始速度の値が有効範囲外です"; return false; }
        if (!IsFinite(fastFallAcceleration) || fastFallAcceleration < 0f) { error = "急降下加速度の値が有効範囲外です"; return false; }
        if (!IsFinite(fastFallTerminalSpeed) || fastFallTerminalSpeed < 0f) { error = "急降下の上限速度の値が有効範囲外です"; return false; }
        if (minPitch > maxPitch) { error = "上下視点の最小角度は最大角度以下にしてください"; return false; }
        error = null;
        return true;
    }
}

