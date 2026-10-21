using System.Collections.Generic;
using InfiniteTileWorld;
using UnityEngine;

public class PhaseWallColor : MonoBehaviour
{
    [System.Serializable]
    private class PhaseRedness
    {
        [Tooltip("色を切り替える基準のフェーズ。StartTime を過ぎたらこの寄せ具合へ移る")]
        public PhaseData phase;

        [Tooltip("赤への寄せ具合。0 で元の色、1 で真っ赤")]
        [Range(0f, 1f)] public float redness;
    }

    private class TintedMaterial
    {
        public readonly Material material;
        public readonly Color lightColor;
        public readonly Color baseColor;

        public TintedMaterial(Material material)
        {
            this.material = material;
            lightColor = material.GetColor(LightColorId);
            baseColor = material.GetColor(BaseColorId);
        }
    }

    private static readonly int LightColorId = Shader.PropertyToID("_LightColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private const float RedHue = 1f;

    [SerializeField] private Timer timer;
    [SerializeField] private StageManager stageManager;

    [Tooltip("フェーズが切り替わったとき、次のフェーズの色へ移るまでの秒数")]
    [SerializeField, Min(0f)] private float jumpSeconds = 0.3f;

    [Tooltip("フェーズ中に、次のフェーズの寄せ具合との差のうち、どれだけを少しずつ進めるか。残りは切り替わりで一気に進む")]
    [SerializeField, Range(0f, 1f)] private float driftRatio = 0.3f;

    [Tooltip("フェーズごとの赤への寄せ具合")]
    [SerializeField] private List<PhaseRedness> phases = new();

    private readonly List<TintedMaterial> tintedMaterials = new();
    private readonly List<PhaseRedness> sortedPhases = new();
    private int currentPhaseIndex;
    private float fromRedness;
    private float currentRedness;
    private float jumpElapsed;
    private bool isJumping;

    private void Start()
    {
        CreateTintedMaterials();

        foreach (var entry in phases)
            if (entry.phase != null) sortedPhases.Add(entry);
        sortedPhases.Sort((a, b) => a.phase.StartTime.CompareTo(b.phase.StartTime));

        currentPhaseIndex = FindPhaseIndex();
        currentRedness = GetPhaseRedness(currentPhaseIndex);
        Apply(currentRedness);
    }

    private void Update()
    {
        int phaseIndex = FindPhaseIndex();
        if (phaseIndex != currentPhaseIndex)
        {
            currentPhaseIndex = phaseIndex;
            fromRedness = currentRedness;
            jumpElapsed = 0f;
            isJumping = true;
        }

        float redness = GetPhaseRedness(phaseIndex);
        if (isJumping)
        {
            jumpElapsed += Time.deltaTime;
            if (jumpElapsed < jumpSeconds)
                redness = Mathf.Lerp(fromRedness, redness, jumpElapsed / jumpSeconds);
            else
                isJumping = false;
        }

        if (redness == currentRedness) return;

        currentRedness = redness;
        Apply(currentRedness);
    }

    private void OnDestroy()
    {
        foreach (var tinted in tintedMaterials)
            Destroy(tinted.material);
        tintedMaterials.Clear();
    }

    private void CreateTintedMaterials()
    {
        var copies = new Dictionary<Material, Material>();

        foreach (var panel in stageManager.Panels)
        {
            if (panel == null) continue;

            foreach (var renderer in panel.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                bool replaced = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    var original = materials[i];
                    if (original == null || !original.HasProperty(LightColorId) || !original.HasProperty(BaseColorId)) continue;

                    if (!copies.TryGetValue(original, out var copy))
                    {
                        copy = new Material(original);
                        copies.Add(original, copy);
                        tintedMaterials.Add(new TintedMaterial(copy));
                    }

                    materials[i] = copy;
                    replaced = true;
                }

                if (replaced) renderer.sharedMaterials = materials;
            }
        }
    }

    private int FindPhaseIndex()
    {
        int index = -1;
        for (int i = 0; i < sortedPhases.Count; i++)
        {
            if (sortedPhases[i].phase.StartTime > timer.ElapsedTime) break;
            index = i;
        }
        return index;
    }

    private float GetPhaseRedness(int index)
    {
        if (index < 0) return 0f;

        var current = sortedPhases[index];
        if (index + 1 >= sortedPhases.Count) return current.redness;

        var next = sortedPhases[index + 1];
        float duration = next.phase.StartTime - current.phase.StartTime;
        float progress = duration > 0f ? Mathf.Clamp01((timer.ElapsedTime - current.phase.StartTime) / duration) : 0f;

        return current.redness + (next.redness - current.redness) * driftRatio * progress;
    }

    private void Apply(float redness)
    {
        foreach (var tinted in tintedMaterials)
        {
            tinted.material.SetColor(LightColorId, ShiftHueToRed(tinted.lightColor, redness));
            tinted.material.SetColor(BaseColorId, ShiftHueToRed(tinted.baseColor, redness));
        }
    }

    private static Color ShiftHueToRed(Color original, float redness)
    {
        Color.RGBToHSV(original, out float hue, out float saturation, out float value);
        Color shifted = Color.HSVToRGB(Mathf.Lerp(hue, RedHue, redness), saturation, value, true);
        shifted.a = original.a;
        return shifted;
    }
}
