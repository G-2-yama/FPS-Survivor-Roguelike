using UnityEngine;

/// <summary>武器モデルのクリップ長から、発射・リロードの再生倍率を設定する。</summary>
[RequireComponent(typeof(Animator))]
public class WeaponAnimationTiming : MonoBehaviour
{
    [SerializeField] private AnimationClip fireClip;
    [SerializeField] private AnimationClip reloadClip;

    private Animator animator;
    private static readonly int FireSpeed = Animator.StringToHash("FireSpeed");
    private static readonly int ReloadSpeed = Animator.StringToHash("ReloadSpeed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetFireDuration(float duration)
    {
        // 低速武器でも発射モーションは引き伸ばさない。
        animator.SetFloat(FireSpeed, Mathf.Max(1f, GetSpeed(fireClip, duration)));
    }

    public void SetReloadDuration(float duration)
    {
        animator.SetFloat(ReloadSpeed, GetSpeed(reloadClip, duration));
    }

    private static float GetSpeed(AnimationClip clip, float duration)
    {
        return clip != null ? clip.length / Mathf.Max(duration, 0.01f) : 1f;
    }
}
