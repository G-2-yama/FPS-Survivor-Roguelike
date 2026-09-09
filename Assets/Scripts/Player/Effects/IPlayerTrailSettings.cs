using UnityEngine;

/// <summary>
/// プレイヤーの軌跡設定を TrailRenderer へ適用するためのインターフェース
/// </summary>
public interface IPlayerTrailSettings
{
    bool IsEnabled { get; }

    /// <summary>
    /// 軌跡設定を指定した TrailRenderer に反映する
    /// </summary>
    /// <param name="trailRenderer">設定対象の TrailRenderer</param>
    void ApplyTo(TrailRenderer trailRenderer);
}
