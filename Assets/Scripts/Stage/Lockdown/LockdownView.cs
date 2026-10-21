using TMPro;
using UnityEngine;

public class LockdownView : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("状態と残り秒数を読む閉じ込めの制御")]
    [SerializeField] private LockdownController controller;

    [Tooltip("閉じ込めの表示全体 何も出さないときは非表示にする")]
    [SerializeField] private GameObject root;

    [Tooltip("告知と残り秒数を出すテキスト")]
    [SerializeField] private TMP_Text messageText;

    [Tooltip("広場の外にいるとき広場の方向を指す矢印 上向きを正面として回す")]
    [SerializeField] private RectTransform arrow;

    [Tooltip("矢印の向きの基準にする視点 未設定ならメインカメラ")]
    [SerializeField] private Transform viewpoint;

    [Header("文言")]
    [Tooltip("警告中に広場の中にいるときの文言 {0} は閉まるまでの秒数")]
    [SerializeField] private string warningFormat = "まもなく広場が封鎖されます　{0}";

    [Tooltip("警告中に広場の外にいるときの文言 {0} は閉まるまでの秒数")]
    [SerializeField] private string outsideFormat = "広場へ戻れ　封鎖まで {0}";

    [Tooltip("閉じている間の文言 {0} は開くまでの秒数")]
    [SerializeField] private string closedFormat = "封鎖中　残り {0}";

    [Tooltip("扉が上がっている間の文言")]
    [SerializeField] private string openingText = "封鎖解除";

    private LockdownController.LockdownState shownState;
    private int shownSeconds = -1;
    private bool shownInside;

    private void Start()
    {
        if (viewpoint == null && Camera.main != null) viewpoint = Camera.main.transform;
        root.SetActive(false);
        arrow.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!controller.isActiveAndEnabled || controller.State == LockdownController.LockdownState.Idle)
        {
            if (root.activeSelf) root.SetActive(false);
            shownState = LockdownController.LockdownState.Idle;
            return;
        }

        if (!root.activeSelf) root.SetActive(true);

        LockdownController.LockdownState state = controller.State;
        bool inside = state != LockdownController.LockdownState.Warning || controller.IsPlayerInside;
        int seconds = state switch
        {
            LockdownController.LockdownState.Warning => Mathf.CeilToInt(controller.SecondsUntilClose),
            LockdownController.LockdownState.Closed => Mathf.CeilToInt(controller.SecondsUntilOpen),
            _ => 0,
        };

        if (state != shownState || seconds != shownSeconds || inside != shownInside)
        {
            shownState = state;
            shownSeconds = seconds;
            shownInside = inside;
            messageText.text = state switch
            {
                LockdownController.LockdownState.Warning => string.Format(inside ? warningFormat : outsideFormat, seconds),
                LockdownController.LockdownState.Closed => string.Format(closedFormat, seconds),
                _ => openingText,
            };
            arrow.gameObject.SetActive(!inside);
        }

        if (!inside) PointArrow();
    }

    private void PointArrow()
    {
        if (viewpoint == null) return;

        Vector3 toPlaza = controller.PlazaCenter - viewpoint.position;
        toPlaza.y = 0f;
        Vector3 forward = viewpoint.forward;
        forward.y = 0f;
        if (toPlaza.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.SignedAngle(forward, toPlaza, Vector3.up);
        arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }
}
