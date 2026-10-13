using System;
using UnityEngine;

/// <summary>
/// プレイヤーのアクション中だけ軌跡を発生させるコンポーネント
/// ステートごとに別の TrailRenderer を使用するため、残っている軌跡の見た目は切り替わらない
/// </summary>
[DisallowMultipleComponent]
public class PlayerActionTrail : MonoBehaviour
{
    /// <summary>専用の軌跡を切り替えるプレイヤーのアクション</summary>
    public enum ActionType
    {
        Dash,
        Slide,
        FastFall
    }
    /// <summary>アクションと専用軌跡の設定を関連付ける</summary>

    [Serializable]
    public class StateBinding
    {
        [Tooltip("この設定を適用するアクションステート")]
        public ActionType action;

        [Tooltip("このステート中に使う軌跡設定 ScriptableObject")]
        public PlayerTrailSettings settings;

        [Tooltip("Editorで事前配置したこのステート専用のTrailRenderer")]
        public TrailRenderer trailRenderer;
    }


    [Tooltip("アクションステートごとの軌跡設定 アクションは重複させないでください")]
    [SerializeField] private StateBinding[] stateBindings;

    private StateBinding activeBinding;
    private readonly StateBinding[] bindings = new StateBinding[(int)ActionType.FastFall + 1];

    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        // 未設定を補完せず、PrefabやSceneの構成ミスとして検出する
        if (stateBindings != null)
            foreach (StateBinding binding in stateBindings)
                if (binding == null || binding.trailRenderer == null)
                    throw new InvalidOperationException($"{name}: 各State BindingのTrail RendererをEditorで設定してください");
        if (stateBindings != null)
            foreach (StateBinding binding in stateBindings)
            {
                int index = (int)binding.action;
                if (index < 0 || index >= bindings.Length)
                    throw new InvalidOperationException($"{name}: 未対応のアクションが設定されています");
                if (bindings[index] == null) bindings[index] = binding;
            }
        StopEmitting();
    }
    /// <summary>無効化された個体の登録と一時状態を解除する</summary>

    private void OnDisable()
    {
        StopEmitting();
    }

    /// <summary>
    /// 指定したアクションステートに紐付いた設定を適用して、専用軌跡の発生を開始する
    /// </summary>
    public void Begin(ActionType action)
    {
        int index = (int)action;
        StateBinding binding = index >= 0 && index < bindings.Length ? bindings[index] : null;
        if (binding == null || binding.settings == null || !binding.settings.IsEnabled)
        {
            StopEmitting();
            return;
        }

        TrailRenderer targetTrailRenderer = binding.trailRenderer;
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
}
