using System;
using UnityEngine;

public enum GameSettingType
{
    MasterVolume,
    BgmVolume,
    SoundEffectVolume,
    MouseLookSensitivity,
    GamepadLookSensitivity
}

public static class GameSettings
{
    public const int MaxLevel = 10;

    private const int LookBaseLevel = 5;
    private const float LookStepRatio = 1.25f;

    private static readonly int[] defaultLevels = { 8, 7, 8, LookBaseLevel, LookBaseLevel };
    private static readonly int[] minLevels = { 0, 0, 0, 1, 1 };

    private static int[] levels = (int[])defaultLevels.Clone();

    public static event Action Changed;

    public static float MouseLookScale => LookScale(GameSettingType.MouseLookSensitivity);

    public static float GamepadLookScale => LookScale(GameSettingType.GamepadLookSensitivity);

    public static int GetLevel(GameSettingType type) => levels[(int)type];

    public static int GetMinLevel(GameSettingType type) => minLevels[(int)type];

    public static void SetLevel(GameSettingType type, int level)
    {
        level = Mathf.Clamp(level, GetMinLevel(type), MaxLevel);
        if (levels[(int)type] == level)
            return;

        levels[(int)type] = level;
        ApplyMasterVolume();
        Changed?.Invoke();
    }

    public static float VolumeScale(SoundPlayType playType)
    {
        var type = playType == SoundPlayType.BGM ? GameSettingType.BgmVolume : GameSettingType.SoundEffectVolume;
        return VolumeOf(type);
    }

    private static float VolumeOf(GameSettingType type) => GetLevel(type) / (float)MaxLevel;

    private static float LookScale(GameSettingType type) => Mathf.Pow(LookStepRatio, GetLevel(type) - LookBaseLevel);

    private static void ApplyMasterVolume()
    {
        AudioListener.volume = VolumeOf(GameSettingType.MasterVolume);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetToDefaults()
    {
        levels = (int[])defaultLevels.Clone();
        Changed = null;
        ApplyMasterVolume();
    }
}
