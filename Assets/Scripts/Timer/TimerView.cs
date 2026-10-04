using UnityEngine;
using UnityEngine.UI;

public class TimerView : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private Timer timer;

    [Tooltip("残り時間の表示形式")]
    [SerializeField] private TimeDisplayFormat displayFormat = TimeDisplayFormat.MinutesSeconds;

    private int displayedTenths = -1;
    private TimeDisplayFormat displayedFormat;

    private void Update()
    {
        if (timer == null) return;

        int remainingTenths = TimeFormatter.ToTenths(timer.RemainingTime);
        if (remainingTenths == displayedTenths && displayFormat == displayedFormat) return;

        displayedTenths = remainingTenths;
        displayedFormat = displayFormat;
        timerText.text = TimeFormatter.FormatTenths(remainingTenths, displayFormat);
    }
}
