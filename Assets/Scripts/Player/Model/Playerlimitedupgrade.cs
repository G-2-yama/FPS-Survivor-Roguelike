using UnityEngine;
using System.Collections.Generic;

public class TimedBuffManager : MonoBehaviour
{
    private readonly List<TimedBuff> activeBuffs
        = new List<TimedBuff>();

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

    public void AddBuff(TimedBuff buff)
    {
        buff.Apply();
        activeBuffs.Add(buff);
    }
}
