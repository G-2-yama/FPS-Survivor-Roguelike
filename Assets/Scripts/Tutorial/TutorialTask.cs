using System;
using UnityEngine;

public enum TutorialGoal
{
    MoveDistance,
    LookAngle,
    AirJump,
    Dash,
    Slide,
    FastFall,
}

[Serializable]
public class TutorialTask
{
    [Tooltip("課題の文。{アクション名} は最後に触った機器のキー名・ボタン名に置き換わる。例: {Move} で移動する")]
    [TextArea(2, 4)]
    [SerializeField] private string template;

    [Tooltip("達成の判定に使う動き")]
    [SerializeField] private TutorialGoal goal;

    [Tooltip("達成に必要な量。移動は m、視点は角度（度）、それ以外は回数")]
    [Min(0.01f)]
    [SerializeField] private float amount = 1f;

    public string Template => template;
    public TutorialGoal Goal => goal;
    public float Amount => amount;
}
