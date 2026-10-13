using UnityEngine;

/// <summary>プレイヤーの操作に応じた視点高さと視野角の演出設定を保持する</summary>
[CreateAssetMenu(fileName = "PlayerCameraSettings", menuName = "Player/設定/カメラ演出")]
public sealed class PlayerCameraSettings : PlayerSettingsAsset
{
    [Header("View Height")]
    [SerializeField, Min(0f), PlayerSettingLabel("Slide Camera Height Multiplier"), Tooltip("通常時のカメラ支点の高さに掛ける倍率")]
    private float slideCameraHeightMultiplier = 0.5f;

    /// <summary>通常時のカメラ支点の高さに掛ける倍率</summary>
    public float SlideCameraHeightMultiplier => slideCameraHeightMultiplier;

    [SerializeField, Min(0f), PlayerSettingLabel("Slide Camera Height Lerp Speed"), Tooltip("視点高さが目標へ近づく速さ 大きいほど素早く変化する")]
    private float slideCameraHeightLerpSpeed = 12f;

    /// <summary>視点高さが目標へ近づく速さ 大きいほど素早く変化する</summary>
    public float SlideCameraHeightLerpSpeed => slideCameraHeightLerpSpeed;

    [Header("Field Of View")]
    [SerializeField, Min(0f), PlayerSettingLabel("Sprint Field Of View Increase"), Tooltip("走行時に通常の視野角へ加える角度 単位は度")]
    private float sprintFieldOfViewIncrease = 6f;

    /// <summary>走行時に通常の視野角へ加える角度 単位は度</summary>
    public float SprintFieldOfViewIncrease => sprintFieldOfViewIncrease;

    [SerializeField, Min(0f), PlayerSettingLabel("Slide Field Of View Increase"), Tooltip("スライド時に通常の視野角へ加える角度 単位は度")]
    private float slideFieldOfViewIncrease = 12f;

    /// <summary>スライド時に通常の視野角へ加える角度 単位は度</summary>
    public float SlideFieldOfViewIncrease => slideFieldOfViewIncrease;

    [SerializeField, Min(0f), PlayerSettingLabel("Field Of View Increase Lerp Speed"), Tooltip("視野角が広がるときの追従速度 大きいほど速く変化する")]
    private float fieldOfViewIncreaseLerpSpeed = 10f;

    /// <summary>視野角が広がるときの追従速度 大きいほど速く変化する</summary>
    public float FieldOfViewIncreaseLerpSpeed => fieldOfViewIncreaseLerpSpeed;

    [SerializeField, Min(0f), PlayerSettingLabel("Field Of View Decrease Lerp Speed"), Tooltip("視野角が元へ戻るときの追従速度 大きいほど速く変化する")]
    private float fieldOfViewDecreaseLerpSpeed = 6f;

    /// <summary>視野角が元へ戻るときの追従速度 大きいほど速く変化する</summary>
    public float FieldOfViewDecreaseLerpSpeed => fieldOfViewDecreaseLerpSpeed;

    /// <summary>カメラ演出の数値と関連する条件を検証する</summary>
    public override bool Validate(out string error)
    {
        if (!IsFinite(slideCameraHeightMultiplier) || slideCameraHeightMultiplier < 0f) { error = "スライド視点高さ倍率の値が有効範囲外です"; return false; }
        if (!IsFinite(slideCameraHeightLerpSpeed) || slideCameraHeightLerpSpeed < 0f) { error = "視点高さの補間速度の値が有効範囲外です"; return false; }
        if (!IsFinite(sprintFieldOfViewIncrease) || sprintFieldOfViewIncrease < 0f) { error = "走行FOV増加の値が有効範囲外です"; return false; }
        if (!IsFinite(slideFieldOfViewIncrease) || slideFieldOfViewIncrease < 0f) { error = "スライドFOV増加の値が有効範囲外です"; return false; }
        if (!IsFinite(fieldOfViewIncreaseLerpSpeed) || fieldOfViewIncreaseLerpSpeed < 0f) { error = "FOV拡大の補間速度の値が有効範囲外です"; return false; }
        if (!IsFinite(fieldOfViewDecreaseLerpSpeed) || fieldOfViewDecreaseLerpSpeed < 0f) { error = "FOV縮小の補間速度の値が有効範囲外です"; return false; }
        error = null;
        return true;
    }
}

