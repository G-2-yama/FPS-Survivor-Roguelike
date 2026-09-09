using UnityEngine;

/// <summary>
/// プレイヤーの1操作分の軌跡設定を保持する ScriptableObject
/// ダッシュ用・スライド用など、操作ごとにアセットを作成して使用する
/// </summary>
[CreateAssetMenu(fileName = "PlayerTrailSettings", menuName = "FPS Survivor/Player/Trail Settings")]
public class PlayerTrailSettings : ScriptableObject, IPlayerTrailSettings
{
    [SerializeField] private bool isEnabled = true;

    [SerializeField]
    [Tooltip("軌跡の先端から末端までの色 アルファでフェード量も調整できます")]
    private Gradient colorOverTrail = CreateDefaultGradient(Color.cyan);

    [SerializeField, Min(0f)]
    [Tooltip("軌跡が残る秒数")]
    private float time = 0.2f;

    [SerializeField, Min(0f)]
    [Tooltip("軌跡の太さ倍率")]
    private float widthMultiplier = 0.35f;

    [SerializeField]
    [Tooltip("軌跡の長さに対する太さの変化")]
    private AnimationCurve widthOverTrail = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [SerializeField, Min(0f)]
    [Tooltip("頂点を追加する最小移動距離 小さいほど滑らかになります")]
    private float minVertexDistance = 0.05f;

    [SerializeField]
    [Tooltip("指定した場合だけ TrailRenderer のマテリアルを切り替えます")]
    private Material material;

    [SerializeField] private LineAlignment alignment = LineAlignment.View;
    [SerializeField] private LineTextureMode textureMode = LineTextureMode.Stretch;

    public bool IsEnabled => isEnabled;

    /// <summary>
    /// このアセットの内容を TrailRenderer へ反映する
    /// </summary>
    public void ApplyTo(TrailRenderer trailRenderer)
    {
        if (trailRenderer == null)
        {
            return;
        }

        trailRenderer.time = time;
        trailRenderer.widthMultiplier = widthMultiplier;
        trailRenderer.widthCurve = widthOverTrail ?? AnimationCurve.Linear(0f, 1f, 1f, 0f);
        trailRenderer.minVertexDistance = minVertexDistance;
        trailRenderer.alignment = alignment;
        trailRenderer.textureMode = textureMode;

        if (colorOverTrail != null)
        {
            trailRenderer.colorGradient = colorOverTrail;
        }

        if (material != null)
        {
            trailRenderer.sharedMaterial = material;
        }
    }

    private static Gradient CreateDefaultGradient(Color color)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        return gradient;
    }
}
