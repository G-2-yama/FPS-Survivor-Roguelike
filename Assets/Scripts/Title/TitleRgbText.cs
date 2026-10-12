using TMPro;
using UnityEngine;

public class TitleRgbText : MonoBehaviour
{
    [Tooltip("元の文字")]
    [SerializeField] private TMP_Text main;

    [Tooltip("左にずらす赤の複製")]
    [SerializeField] private TMP_Text red;

    [Tooltip("右にずらすシアンの複製")]
    [SerializeField] private TMP_Text cyan;

    [Tooltip("複製をずらす幅（px）")]
    [SerializeField] private float split = 4f;

    [Tooltip("色ずれが始まったときの揺れの時間（秒）")]
    [SerializeField] private float jitterDuration = 0.3f;

    private RectTransform rect;
    private Vector2 home;
    private bool isPlaying;
    private float startedAt;

    private void Awake()
    {
        rect = (RectTransform)transform;
        home = rect.anchoredPosition;
        red.rectTransform.anchoredPosition = new Vector2(-split, 0f);
        cyan.rectTransform.anchoredPosition = new Vector2(split, 0f);
        SetCopiesActive(false);
    }

    public void Play()
    {
        isPlaying = true;
        startedAt = Time.unscaledTime;
        SetCopiesActive(true);
        SyncCopies();
    }

    public void Stop()
    {
        isPlaying = false;
        SetCopiesActive(false);
        rect.anchoredPosition = home;
    }

    private void LateUpdate()
    {
        if (!isPlaying)
            return;

        SyncCopies();
        float t = (Time.unscaledTime - startedAt) / jitterDuration;
        rect.anchoredPosition = home + new Vector2(JitterOffset(t), 0f);
    }

    private static float JitterOffset(float t)
    {
        if (t < 0.3f)
            return -6f;
        if (t < 0.6f)
            return 5f;
        if (t < 1f)
            return -2f;
        return 0f;
    }

    private void SyncCopies()
    {
        SyncCopy(red);
        SyncCopy(cyan);
    }

    private void SyncCopy(TMP_Text copy)
    {
        if (copy.text != main.text)
            copy.text = main.text;
        if (!Mathf.Approximately(copy.fontSize, main.fontSize))
            copy.fontSize = main.fontSize;
    }

    private void SetCopiesActive(bool isActive)
    {
        red.gameObject.SetActive(isActive);
        cyan.gameObject.SetActive(isActive);
    }
}
