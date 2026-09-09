using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// プレイヤーのアクション中だけ軌跡を発生させるコンポーネント
/// ステートごとに別の TrailRenderer を使用するため、残っている軌跡の見た目は切り替わらない
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TrailRenderer))]
public class PlayerActionTrail : MonoBehaviour
{
    public enum ActionType
    {
        Dash,
        Slide,
        FastFall
    }

    [Serializable]
    public class StateBinding
    {
        [Tooltip("この設定を適用するアクションステート")]
        public ActionType action;

        [Tooltip("このステート中に使う軌跡設定 ScriptableObject")]
        public PlayerTrailSettings settings;

        [Tooltip("このステート専用の TrailRenderer 未設定の場合は実行時に自動生成されます")]
        public TrailRenderer trailRenderer;
    }

    [FormerlySerializedAs("trailRenderer")]
    [SerializeField] private TrailRenderer primaryTrailRenderer;

    [Tooltip("アクションステートごとの軌跡設定 アクションは重複させないでください")]
    [SerializeField] private StateBinding[] stateBindings;

    private StateBinding activeBinding;

    private void Reset()
    {
        primaryTrailRenderer = GetComponent<TrailRenderer>();
    }

    private void Awake()
    {
        if (primaryTrailRenderer == null)
        {
            primaryTrailRenderer = GetComponent<TrailRenderer>();
        }

        EnsureTrailRenderers();
        StopEmitting();
    }

    private void OnDisable()
    {
        StopEmitting();
    }

    /// <summary>
    /// 指定したアクションステートに紐付いた設定を適用して、専用軌跡の発生を開始する
    /// </summary>
    public void Begin(ActionType action)
    {
        StateBinding binding = FindBinding(action);
        if (binding == null || binding.settings == null || !binding.settings.IsEnabled)
        {
            StopEmitting();
            return;
        }

        TrailRenderer targetTrailRenderer = GetOrCreateTrailRenderer(binding);
        if (targetTrailRenderer == null)
        {
            StopEmitting();
            return;
        }

        if (activeBinding != null
            && activeBinding != binding
            && activeBinding.trailRenderer != null)
        {
            activeBinding.trailRenderer.emitting = false;
        }

        activeBinding = binding;
        binding.settings.ApplyTo(targetTrailRenderer);
        targetTrailRenderer.emitting = true;
    }

    /// <summary>
    /// 指定したアクションの軌跡だけを停止する
    /// すでに生成された部分はそのステート専用の設定のまま Time が経過するまで残る
    /// </summary>
    public void End(ActionType action)
    {
        if (activeBinding == null || activeBinding.action != action)
        {
            return;
        }

        if (activeBinding.trailRenderer != null)
        {
            activeBinding.trailRenderer.emitting = false;
        }

        activeBinding = null;
    }

    /// <summary>
    /// すべての軌跡の発生を停止する
    /// </summary>
    public void StopEmitting()
    {
        activeBinding = null;

        if (stateBindings == null)
        {
            return;
        }

        for (int i = 0; i < stateBindings.Length; i++)
        {
            TrailRenderer targetTrailRenderer = stateBindings[i]?.trailRenderer;
            if (targetTrailRenderer != null)
            {
                targetTrailRenderer.emitting = false;
            }
        }
    }

    private void EnsureTrailRenderers()
    {
        if (stateBindings == null)
        {
            return;
        }

        bool isPrimaryTrailRendererAssigned = false;
        for (int i = 0; i < stateBindings.Length; i++)
        {
            StateBinding binding = stateBindings[i];
            if (binding?.trailRenderer == primaryTrailRenderer)
            {
                isPrimaryTrailRendererAssigned = true;
                break;
            }
        }

        for (int i = 0; i < stateBindings.Length; i++)
        {
            StateBinding binding = stateBindings[i];
            if (binding == null || binding.trailRenderer != null)
            {
                continue;
            }

            if (!isPrimaryTrailRendererAssigned && primaryTrailRenderer != null)
            {
                binding.trailRenderer = primaryTrailRenderer;
                isPrimaryTrailRendererAssigned = true;
                continue;
            }

            binding.trailRenderer = CreateTrailRenderer(binding.action);
        }
    }

    private TrailRenderer GetOrCreateTrailRenderer(StateBinding binding)
    {
        if (binding.trailRenderer != null)
        {
            return binding.trailRenderer;
        }

        EnsureTrailRenderers();
        return binding.trailRenderer;
    }

    private TrailRenderer CreateTrailRenderer(ActionType action)
    {
        string objectName = $"Player Trail - {action}";
        Transform existingChild = transform.Find(objectName);
        if (existingChild != null && existingChild.TryGetComponent(out TrailRenderer existingTrailRenderer))
        {
            return existingTrailRenderer;
        }

        GameObject trailObject = new GameObject(objectName);
        trailObject.transform.SetParent(transform, false);

        TrailRenderer newTrailRenderer = trailObject.AddComponent<TrailRenderer>();
        newTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        newTrailRenderer.receiveShadows = false;
        newTrailRenderer.emitting = false;
        return newTrailRenderer;
    }

    private StateBinding FindBinding(ActionType action)
    {
        if (stateBindings == null)
        {
            return null;
        }

        for (int i = 0; i < stateBindings.Length; i++)
        {
            StateBinding binding = stateBindings[i];
            if (binding != null && binding.action == action)
            {
                return binding;
            }
        }

        return null;
    }
}
