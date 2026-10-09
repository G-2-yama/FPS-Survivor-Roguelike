using UnityEngine;
using TMPro;

public class TimerView : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Timer timer;

    [Tooltip("残り時間（エンドレスでは経過時間）の表示形式")]
    [SerializeField] private TimeDisplayFormat displayFormat = TimeDisplayFormat.MinutesSeconds;

    private int displayedTenths = -1;
    private TimeDisplayFormat displayedFormat;

    private void Update()
    {
        if (timer == null) return;

        float seconds = timer.IsUnlimited ? timer.ElapsedTime : timer.RemainingTime;
        int tenths = TimeFormatter.ToTenths(seconds);
        if (tenths == displayedTenths && displayFormat == displayedFormat) return;

        displayedTenths = tenths;
        displayedFormat = displayFormat;
        timerText.text = TimeFormatter.FormatTenths(tenths, displayFormat);
    }
}
