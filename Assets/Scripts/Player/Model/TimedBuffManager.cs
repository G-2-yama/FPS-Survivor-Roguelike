using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>プレイヤー専用の時間付き効果を管理し、表示側へ変更を通知する。</summary>
[DisallowMultipleComponent]
public class TimedBuffManager : MonoBehaviour
{
    private readonly List<TimedBuff> activeBuffs = new List<TimedBuff>();
    public IReadOnlyList<TimedBuff> ActiveBuffs => activeBuffs;
    public event Action BuffsChanged;

    private void Update() => Tick(Time.deltaTime);

    public void Tick(float deltaTime)
    {
        bool changed = false;
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            TimedBuff buff = activeBuffs[i];
            buff.Update(deltaTime);
            if (!buff.IsFinished) continue;
            buff.Remove();
            activeBuffs.RemoveAt(i);
            changed = true;
        }
        if (changed) BuffsChanged?.Invoke();
    }

    public void AddBuff(TimedBuff buff)
    {
        if (buff == null) throw new ArgumentNullException(nameof(buff));
        foreach (TimedBuff activeBuff in activeBuffs)
        {
            if (!activeBuff.TryMerge(buff)) continue;
            BuffsChanged?.Invoke();
            return;
        }
        buff.Apply();
        activeBuffs.Add(buff);
        BuffsChanged?.Invoke();
    }

    public void ClearBuffs()
    {
        foreach (var buff in activeBuffs) buff.Remove();
        activeBuffs.Clear();
        BuffsChanged?.Invoke();
    }

    private void OnDestroy() => ClearBuffs();
}