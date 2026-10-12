using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TitleColorTarget
{
    [Tooltip("色を変える対象")]
    [SerializeField] private Graphic graphic;

    [Tooltip("通常時の色")]
    [SerializeField] private Color normal = Color.white;

    [Tooltip("ホバー中・選択中の色")]
    [SerializeField] private Color active = Color.white;

    public void Apply(float progress)
    {
        if (graphic != null)
            graphic.color = Color.Lerp(normal, active, progress);
    }
}
