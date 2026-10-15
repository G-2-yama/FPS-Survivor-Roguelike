using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("拡大倍率")]
    [SerializeField] private float hoverScale = 1.1f;

    [Header("拡大速度")]
    [SerializeField] private float speed = 10f;

    private Vector3 defaultScale;
    private Vector3 targetScale;

    private void Awake()
    {
        defaultScale = transform.localScale;
        targetScale = defaultScale;
    }

    /// <summary>
    /// UIが表示されるたびにスケールをリセットする
    /// </summary>
    private void OnEnable()
    {
        transform.localScale = defaultScale;
        targetScale = defaultScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * speed
        );
    }

    /// <summary>
    /// マウスが乗った時
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetPointerFocus(true);
    }

    /// <summary>
    /// マウスが離れた時
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        SetPointerFocus(false);
    }

    /// <summary>
    /// ポインター操作以外から指定されたフォーカス状態を反映する
    /// </summary>
    public void SetPointerFocus(bool isFocused)
    {
        targetScale = isFocused ? defaultScale * hoverScale : defaultScale;
    }
}
