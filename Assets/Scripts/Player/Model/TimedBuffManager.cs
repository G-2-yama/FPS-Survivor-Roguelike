using UnityEngine;
using System.Collections.Generic;

/// <summary>プレイヤーに属する時間制限付き強化を更新する</summary>
[DisallowMultipleComponent]
public class TimedBuffManager : MonoBehaviour
{
    private readonly List<TimedBuff> activeBuffs
        = new List<TimedBuff>();

    /// <summary>強化の残り時間を進め、終了した効果を解除する</summary>
    private void Update()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            TimedBuff buff = activeBuffs[i];

            buff.Update(Time.deltaTime);

            if (buff.IsFinished)
            {
                buff.Remove();
                activeBuffs.RemoveAt(i);
            }
        }
    }

    /// <summary>強化を適用して期限管理の対象に追加する</summary>
    public void AddBuff(TimedBuff buff)
    {
        buff.Apply();
        activeBuffs.Add(buff);
    }
}
