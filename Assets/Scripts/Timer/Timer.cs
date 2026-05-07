using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField] private float timeLimit = 60f;

    public float TimeLimit => timeLimit;
    public float ElapsedTime { get; private set; }
    public float RemainingTime => Mathf.Max(0f, timeLimit - ElapsedTime);
    public bool IsRunning { get; private set; }
    public bool IsFinished => ElapsedTime >= timeLimit;

    public event UnityAction OnTimerFinished;

    public void StartTimer()
    {
        IsRunning = true;
    }

    public void RestartTimer()
    {
        ElapsedTime = 0f;
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    private void Update()
    {
        if (!IsRunning) return;

        ElapsedTime += Time.deltaTime;

        if (IsFinished)
        {
            IsRunning = false;
            OnTimerFinished?.Invoke();
        }
    }
}