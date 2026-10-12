using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PooledSoundPlayer : PoolableObject
{
    private const int MaxPlayingPerClip = 8;

    private static readonly Dictionary<AudioClip, int> playingCounts = new();

    [SerializeField] private AudioSource audioSource;

    private Coroutine playCoroutine;

    private AudioClip countedClip;

    public static bool CanPlay(AudioClip clip)
    {
        return clip != null
            && (!playingCounts.TryGetValue(clip, out int count) || count < MaxPlayingPerClip);
    }

    private void CountPlaying(AudioClip clip)
    {
        countedClip = clip;
        playingCounts.TryGetValue(clip, out int count);
        playingCounts[clip] = count + 1;
    }

    private void UncountPlaying()
    {
        if (countedClip == null)
        {
            return;
        }

        if (playingCounts.TryGetValue(countedClip, out int count))
        {
            if (count <= 1)
            {
                playingCounts.Remove(countedClip);
            }
            else
            {
                playingCounts[countedClip] = count - 1;
            }
        }

        countedClip = null;
    }

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Play(SoundData data)
    {
       
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        audioSource.Stop();
        UncountPlaying();

        if (data == null || data.clip == null)
        {
            Release();
            return;
        }

        ApplySetting(data);

        audioSource.clip = data.clip;
        audioSource.loop = false;

       

        audioSource.Play();
        CountPlaying(data.clip);

        playCoroutine = StartCoroutine(WaitForSoundEnd());
    }

    private IEnumerator WaitForSoundEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);

        playCoroutine = null;
        UncountPlaying();

      

        Release();
    }

    private void ApplySetting(SoundData data)
    {
        audioSource.volume = data.volume;
        audioSource.pitch = data.pitch;

        audioSource.spatialBlend =
            data.is3D ? data.spatialBlend : 0f;
    }

    public override void OnGet()
    {
       
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
        UncountPlaying();


    }

    public override void OnRelease()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
        UncountPlaying();


    }

    private void OnDestroy()
    {
        UncountPlaying();
    }
}