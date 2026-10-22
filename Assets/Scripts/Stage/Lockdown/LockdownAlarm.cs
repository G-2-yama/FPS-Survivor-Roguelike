using UnityEngine;

public class LockdownAlarm : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("状態と残り秒数を読む閉じ込めの制御")]
    [SerializeField] private LockdownController controller;

    [Tooltip("警告音を鳴らすSounder BGMを鳴らしているSounderとは別のものを入れる")]
    [SerializeField] private Sounder sounder;

    [Header("音")]
    [Tooltip("警告音のカテゴリ")]
    [SerializeField] private SoundCategory category = SoundCategory.Environment;

    [Tooltip("警告中に1秒ごとに鳴らす音の番号")]
    [SerializeField] private int countdownIndex = 0;

    [Tooltip("扉が閉まったときに鳴らす音の番号")]
    [SerializeField] private int closeIndex = 1;

    [Tooltip("扉が開き始めたときに鳴らす音の番号")]
    [SerializeField] private int openIndex = 2;

    private int lastCountdownSecond = -1;

    private void OnEnable()
    {
        controller.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        controller.StateChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (controller.State != LockdownController.LockdownState.Warning) return;

        int second = Mathf.CeilToInt(controller.SecondsUntilClose);
        if (second == lastCountdownSecond || second <= 0) return;

        lastCountdownSecond = second;
        sounder.Play(category, countdownIndex);
    }

    private void OnStateChanged(LockdownController.LockdownState state)
    {
        lastCountdownSecond = -1;

        switch (state)
        {
            case LockdownController.LockdownState.Closed:
                sounder.Play(category, closeIndex);
                break;
            case LockdownController.LockdownState.Opening:
                sounder.Play(category, openIndex);
                break;
        }
    }
}
