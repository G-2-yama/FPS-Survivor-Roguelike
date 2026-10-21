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
    WeaponHit,
    Reload,
    Confirm,
}

[Serializable]
public class TutorialEquip
{
    [Tooltip("持たせるスロット")]
    [SerializeField] private SlotType slot;

    [Tooltip("持たせる武器")]
    [SerializeField] private WeaponData weapon;

    public SlotType Slot => slot;
    public WeaponData Weapon => weapon;
}

[Serializable]
public class TutorialTask
{
    [Tooltip("課題の文。{アクション名} は最後に触った機器のキー名・ボタン名に置き換わる。例: {Move} で移動する")]
    [TextArea(2, 4)]
    [SerializeField] private string template;

    [Tooltip("達成の判定に使う動き。Confirm は Interact を押すと次へ進む")]
    [SerializeField] private TutorialGoal goal;

    [Tooltip("達成に必要な量。移動は m、視点は角度（度）、それ以外は回数")]
    [Min(0.01f)]
    [SerializeField] private float amount = 1f;

    [Tooltip("命中・リロードの判定で見る武器のスロット。命中はこのスロットの武器が撃つまで数えない。リロードはこのスロットのリロード操作を押した回数（LeftMain・RightMain のみ）")]
    [SerializeField] private SlotType slot;

    [Tooltip("課題を始めるときに持たせる武器")]
    [SerializeField] private TutorialEquip[] equips = Array.Empty<TutorialEquip>();

    [Tooltip("枠で指し示す HUD。GameScene の UICanvas からの階層パス（例: Character/HealthBar）。複数あるときは全部を囲む")]
    [SerializeField] private string[] highlightPaths = Array.Empty<string>();

    public string Template => template;
    public TutorialGoal Goal => goal;
    public float Amount => amount;
    public SlotType Slot => slot;
    public TutorialEquip[] Equips => equips;
    public string[] HighlightPaths => highlightPaths;
}
