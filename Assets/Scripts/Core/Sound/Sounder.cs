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

    /// <summary>
    /// 指定したSoundCategoryの音を再生する
    /// </summary>
    /// <param name="category">再生する音のカテゴリ</param>
    /// <param name="index">再生する音のインデックス</param>
    public void Play(SoundCategory category, int index = -1)
    {

        SoundData data;

        // indexが-1なら、そのCategoryからランダム選択
        if (index == -1)
        {
            List<SoundData> candidates = new();

            foreach (SoundData sound in soundDictionary.Values)
            {
                if (sound.category == category)
                {
                    candidates.Add(sound);
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"Sound Not Found : {category}");
                return;
            }

            data = candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            if (!soundDictionary.TryGetValue((category, index), out data))
            {
                Debug.LogWarning($"Sound Not Found : {category} : {index}");
                return;
            }
        }

        ApplySetting(data);

        // 再生方法に応じて再生
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

    /// <summary>
    /// SoundDataの設定をAudioSourceに適用する
    /// </summary>
    /// <param name="data">適用するSoundData</param>
    private void ApplySetting(SoundData data)
    {
        audioSource.volume = data.volume;

        audioSource.pitch = data.pitch;

        audioSource.spatialBlend = data.is3D ? data.spatialBlend : 0f;

        audioSource.clip = data.clip;
    }
}