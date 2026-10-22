using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LockdownSchedule", menuName = "Stage/閉じ込めの予定表")]
public class LockdownSchedule : ScriptableObject
{
    [Tooltip("閉じ込めの予定 閉じる時刻の早い順に並べる")]
    [SerializeField] private List<LockdownEntry> entries = new List<LockdownEntry>();

    public IReadOnlyList<LockdownEntry> Entries => entries;
}

[Serializable]
public class LockdownEntry
{
    [Tooltip("扉が閉まる時刻（ゲーム開始からの経過秒）")]
    [SerializeField, Min(0f)] private float closeTime = 120f;

    [Tooltip("扉が閉まっている長さ（秒）")]
    [SerializeField, Min(0f)] private float duration = 30f;

    [Tooltip("閉まる何秒前から警告するか")]
    [SerializeField, Min(0f)] private float warningSeconds = 10f;

    public float CloseTime => closeTime;
    public float Duration => duration;
    public float WarningSeconds => warningSeconds;
    public float WarningStartTime => Mathf.Max(0f, closeTime - warningSeconds);
    public float OpenTime => closeTime + duration;
}
