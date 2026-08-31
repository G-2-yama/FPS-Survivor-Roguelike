using UnityEngine;

public enum SoundPlayType
{
    OneShot,
    Loop,
    BGM,
}

public enum SoundCategory
{
    BGM,
    Fire,
    Charge,
    ReloadEnter,
    ReloadEnd,
    Player,
    Enemy,
    Environment,
    UI,
    Voice,
    GetItem,
}

[System.Serializable]
public class SoundData
{
    [Header("ID")]
    public SoundCategory category;
    public int index;

    [Header("Clip")]
    public AudioClip clip;

    [Header("Volume")]
    [Range(0, 1)]
    public float volume = 1f;

    [Header("Pitch")]
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Header("Play Type")]
    public SoundPlayType playType;

    [Header("3D Sound")]
    public bool is3D = false;

    [Range(0, 1)]
    public float spatialBlend = 1f;

    [Header("Loop")]
    public bool loop = false;
}