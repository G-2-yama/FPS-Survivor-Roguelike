using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PooledSoundPlayer : PoolableObject
{
    [SerializeField] private AudioSource audioSource;

    private Coroutine playCoroutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Play(SoundData data)
    {
        if (data == null || data.clip == null)
        {
            Release();
            return;
        }

        ApplySetting(data);

        audioSource.clip = data.clip;
        audioSource.loop = false;

        audioSource.Play();

        playCoroutine = StartCoroutine(WaitForSoundEnd());
    }

    private IEnumerator WaitForSoundEnd()
    {
        // AudioSource‚ªÄ¶I—¹‚·‚é‚Ü‚Å‘Ò‚Â
        yield return new WaitWhile(() => audioSource.isPlaying);

        playCoroutine = null;

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
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
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
    }
}
