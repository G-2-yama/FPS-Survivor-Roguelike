using System.Collections.Generic;
using UnityEngine;
/// <summary>効果音の選択と再生方法を管理する</summary>

[RequireComponent(typeof(AudioSource))]
public class Sounder : MonoBehaviour
{
    [SerializeField] private SoundDB soundDatabase;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject soundprefab;

    private Dictionary<(SoundCategory, int), SoundData> soundDictionary;
    private Dictionary<SoundCategory, List<SoundData>> categoryCandidates;
    private SoundData loopingData;
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        InitializeDictionary();
    }

    private void OnEnable()
    {
        GameSettings.Changed += ApplyLoopingVolume;
    }

    private void OnDisable()
    {
        GameSettings.Changed -= ApplyLoopingVolume;
    }

    private void ApplyLoopingVolume()
    {
        if (loopingData != null)
            audioSource.volume = loopingData.volume * GameSettings.VolumeScale(loopingData.playType);
    }
    /// <summary>効果音設定をカテゴリと番号で参照できるよう準備する</summary>

    private void InitializeDictionary()
    {
        soundDictionary = new();
        categoryCandidates = new();

        if(soundDatabase == null)
        {
            return;
        }
        
        foreach (SoundData data in soundDatabase.sounds)
        {
            soundDictionary[(data.category, data.index)] = data;
        }

        foreach (SoundData data in soundDictionary.Values)
        {
            if (!categoryCandidates.TryGetValue(data.category, out List<SoundData> candidates))
            {
                candidates = new();
                categoryCandidates[data.category] = candidates;
            }

            candidates.Add(data);
        }
    }
    /// <summary>新しい効果音設定を反映する</summary>

    public void SetSoundDB(SoundDB newDB)
    {
        soundDatabase = newDB;
        InitializeDictionary();
    }
    /// <summary>保持済みの部品参照を使って位置付きの効果音を再生する</summary>
    private void PlayPooledSound(SoundData data)
    {
        if (!PooledSoundPlayer.CanPlay(data.clip))
        {
            return;
        }

        PooledInstance spawned = PoolManager.Instance.Rent(soundprefab);
        GameObject obj = spawned.GameObject;

        obj.transform.position = transform.position;
        obj.transform.rotation = Quaternion.identity;

        PooledSoundPlayer player =
            spawned.RequireSoundPlayer();

        player.Play(data);
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
            // 候補がない場合は何もせずに終了
            if (!categoryCandidates.TryGetValue(category, out List<SoundData> candidates))
            {
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
            case SoundPlayType.PlayClipAtPoint:
                PlayPooledSound(data);
                break;

            case SoundPlayType.Loop:
            case SoundPlayType.BGM:

                audioSource.clip = data.clip;
                audioSource.loop = true;
                audioSource.Play();
                loopingData = data;

                break;
        }
    }
    /// <summary>現在の効果音の再生を停止する</summary>

    public void Stop()
    {
        audioSource.Stop();
        loopingData = null;
    }

    /// <summary>
    /// SoundDataの設定をAudioSourceに適用する
    /// </summary>
    /// <param name="data">適用するSoundData</param>
    private void ApplySetting(SoundData data)
    {
        audioSource.volume = data.volume * GameSettings.VolumeScale(data.playType);

        audioSource.pitch = data.pitch;

        audioSource.spatialBlend = data.is3D ? data.spatialBlend : 0f;

        audioSource.clip = data.clip;
    }
}