using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのアクション状態に合わせて集中線の表示を制御するコンポーネント
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RawImage))]
public sealed class PlayerActionConcentrationLine : MonoBehaviour
{
    /// <summary>
    /// 集中線を描画する RawImage
    /// </summary>
    [SerializeField] private RawImage concentrationLine;

    /// <summary>
    /// 現在集中線を表示しているアクション
    /// </summary>
    private ActionType? activeAction;

    /// <summary>
    /// 集中線を表示できるプレイヤーアクションの種類
    /// </summary>
    public enum ActionType
    {
        Dash,
        Slide,
        FastFall
    }

    /// <summary>
    /// コンポーネント追加時に同じ GameObject の RawImage を設定する
    /// </summary>
    private void Reset()
    {
        concentrationLine = GetComponent<RawImage>();
    }

    /// <summary>
    /// 必須参照を取得し、集中線を非表示で初期化する
    /// </summary>
    private void Awake()
    {
        if (concentrationLine == null)
        {
            concentrationLine = GetComponent<RawImage>();
        }

        SetVisible(false);
    }

    /// <summary>
    /// 指定したアクションの集中線表示を開始する
    /// </summary>
    /// <param name="action">開始したプレイヤーアクション</param>
    public void Begin(ActionType action)
    {
        activeAction = action;
        SetVisible(true);
    }

    /// <summary>
    /// 指定したアクションが現在の表示元であれば集中線を非表示にする
    /// </summary>
    /// <param name="action">終了したプレイヤーアクション</param>
    public void End(ActionType action)
    {
        if (!activeAction.HasValue || activeAction.Value != action)
        {
            return;
        }

        activeAction = null;
        SetVisible(false);
    }

    /// <summary>
    /// 集中線の状態をリセットして非表示にする
    /// </summary>
    public void Stop()
    {
        activeAction = null;
        SetVisible(false);
    }

    /// <summary>
    /// コンポーネントが無効になったときに集中線を非表示にする
    /// </summary>
    private void OnDisable()
    {
        Stop();
    }

    /// <summary>
    /// RawImage の有効状態を変更する
    /// </summary>
    /// <param name="isVisible">集中線を表示するかどうか</param>
    private void SetVisible(bool isVisible)
    {
        if (concentrationLine != null)
        {
            concentrationLine.enabled = isVisible;
        }
    }
}
