using System;
using UnityEngine;

/// <summary>開始時の設定をプレイヤー専用に複製して保持する</summary>
public sealed class PlayerSettingsSnapshot : IDisposable
{
    public PlayerStatsSettings Stats { get; }
    public PlayerControlSettings Control { get; }
    public PlayerCollisionSettings Collision { get; }
    public PlayerCameraSettings Camera { get; }
    public PlayerProgressionSettings Progression { get; }

    /// <summary>各設定を複製し、プレイ中のアセット編集による意図しない変化を防ぐ</summary>
    public PlayerSettingsSnapshot(PlayerConfig config)
    {
        Stats = Copy(config.Stats);
        Control = Copy(config.Control);
        Collision = Copy(config.Collision);
        Camera = Copy(config.Camera);
        Progression = Copy(config.Progression);
    }

    /// <summary>保存されない実行時専用のSOを生成する</summary>
    private static T Copy<T>(T asset) where T : ScriptableObject
    {
        T copy = UnityEngine.Object.Instantiate(asset);
        copy.hideFlags = HideFlags.HideAndDontSave;
        return copy;
    }

    /// <summary>プレイヤーの終了時に複製した設定を破棄する</summary>
    public void Dispose()
    {
        Release(Stats);
        Release(Control);
        Release(Collision);
        Release(Camera);
        Release(Progression);
    }

    /// <summary>実行環境に合わせて設定の複製を破棄する</summary>
    private static void Release(UnityEngine.Object instance)
    {
        if (instance == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(instance);
        else UnityEngine.Object.DestroyImmediate(instance);
    }
}