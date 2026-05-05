using UnityEngine;
using UnityEngine.UI;

public class TimerView : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private Timer timer;

    private void Update()
    {
        if (timer == null) return;
        timerText.text = $"{timer.RemainingTime:F1}";
    }
}