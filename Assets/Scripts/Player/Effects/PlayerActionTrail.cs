using System;
using UnityEngine;

/// <summary>
/// プレイヤーのアクション中だけ TrailRenderer を発生させるコンポーネント
/// Inspector でステートと PlayerTrailSettings アセットを対応付ける
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

    /// <summary>
    /// アクションステートと軌跡設定アセットの対応
    /// </summary>
    [Serializable]
    public class StateBinding
    {
        [Tooltip("この設定を適用するアクションステート")]
        public ActionType action;

        [Tooltip("このステート中に使う軌跡設定 ScriptableObject")]
        public PlayerTrailSettings settings;
    }

    [SerializeField] private TrailRenderer trailRenderer;

    [Tooltip("アクションステートごとの軌跡設定 アクションは重複させないでください")]
    [SerializeField] private StateBinding[] stateBindings;

    private ActionType? activeAction;

    public TrailRenderer TrailRenderer => trailRenderer;

    private void Reset()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Awake()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        StopEmitting(clear: true);
    }

    private void OnDisable()
    {
        StopEmitting(clear: true);
    }

    /// <summary>
    /// 指定したアクションステートに紐付いた設定を適用して、軌跡の発生を開始する
    /// </summary>
    public void Begin(ActionType action)
    {
        IPlayerTrailSettings settings = FindSettings(action);
        activeAction = action;

        if (settings == null || !settings.IsEnabled || trailRenderer == null)
        {
            StopEmitting(clear: false);
            return;
        }

        settings.ApplyTo(trailRenderer);
        trailRenderer.Clear();
        trailRenderer.emitting = true;
    }

    /// <summary>
    /// 指定したアクションの軌跡を停止する すでに生成された軌跡は設定アセットの Time だけ残る
    /// </summary>
    public void End(ActionType action)
    {
        if (activeAction != action)
        {
            return;
        }

        activeAction = null;
        StopEmitting(clear: false);
    }

    /// <summary>
    /// すべての軌跡の発生を停止する
    /// </summary>
    public void StopEmitting(bool clear)
    {
        activeAction = null;

        if (trailRenderer == null)
        {
            return;
        }

        trailRenderer.emitting = false;
        if (clear)
        {
            trailRenderer.Clear();
        }
    }

    private IPlayerTrailSettings FindSettings(ActionType action)
    {
        if (stateBindings == null)
        {
            return null;
        }

        for (int i = 0; i < stateBindings.Length; i++)
        {
            StateBinding binding = stateBindings[i];
            if (binding != null && binding.action == action && binding.settings != null)
            {
                return binding.settings;
            }
        }

        return null;
    }
}
