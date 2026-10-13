using UnityEngine;

public class ExplosionHeightFade : MonoBehaviour
{
    private static readonly int FadeBaseYId = Shader.PropertyToID("_FadeBaseY");
    private static readonly int FadeHeightId = Shader.PropertyToID("_FadeHeight");
    private static readonly int OpacityId = Shader.PropertyToID("_Opacity");

    [Tooltip("爆心からこの高さ(m)まで上がると透明になりきります 0 で高さによる変化をなくします")]
    [SerializeField, Min(0f)] private float fadeHeight = 4f;

    [Tooltip("爆発全体の濃さ 1 で元の濃さ")]
    [SerializeField, Range(0f, 1f)] private float opacity = 0.7f;

    private ParticleSystemRenderer[] renderers;
    private MaterialPropertyBlock block;

    private void Awake()
    {
        renderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
        block = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        block.SetFloat(FadeBaseYId, transform.position.y);
        block.SetFloat(FadeHeightId, fadeHeight);
        block.SetFloat(OpacityId, opacity);
        foreach (ParticleSystemRenderer particleRenderer in renderers)
        {
            particleRenderer.SetPropertyBlock(block);
        }
    }
}
