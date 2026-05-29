using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sounder : MonoBehaviour
{
    [SerializeField] private SoundDB soundDatabase;

    [SerializeField] private AudioSource audioSource;

    private Dictionary<(SoundCategory, int), SoundData> soundDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new();

        if(soundDatabase == null)
        {
            return;
        }
        
        foreach (SoundData data in soundDatabase.sounds)
        {
            soundDictionary[(data.category, data.index)] = data;
        }
    }

    public void SetSoundDB(SoundDB newDB)
    {
        soundDatabase = newDB;
        InitializeDictionary();
    }

    public void Play(SoundCategory category, int index = 0)
    {
        if (!soundDictionary.TryGetValue((category, index), out SoundData data))
        {
            Debug.LogWarning($"Sound Not Found : {category} : {index}");
            return;
        }

        //ApplySetting(data);

        switch (data.playType)
        {
            case SoundPlayType.OneShot:
                audioSource.PlayOneShot(data.clip, data.volume);
                break;

            case SoundPlayType.Loop:
            case SoundPlayType.BGM:

                audioSource.clip = data.clip;
                audioSource.loop = true;
                audioSource.Play();

                break;
        }
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    private void ApplySetting(SoundData data)
    {
        audioSource.volume = data.volume;

        audioSource.pitch = data.pitch;

        audioSource.spatialBlend = data.is3D ? data.spatialBlend : 0f;

        audioSource.clip = data.clip;
    }
}