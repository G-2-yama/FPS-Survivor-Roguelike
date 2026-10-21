using UnityEngine;

/// <summary>
/// UI用カーソルの移動速度と見た目を保持する設定
/// </summary>
[CreateAssetMenu(fileName = "GameUICursorSettings", menuName = "Game/UI Cursor Settings")]
public sealed class GameUICursorSettings : ScriptableObject
{
    [InspectorName("D-Pad Speed")]
    [Tooltip("ゲームパッドの十字キーでカーソルを動かす速度")]
    [SerializeField, Min(0f)] private float dpadSpeed = 480f;

    [InspectorName("Stick Speed")]
    [Tooltip("ゲームパッドの左スティックでカーソルを動かす速度")]
    [SerializeField, Min(0f)] private float stickSpeed = 960f;

    [InspectorName("Cursor Size")]
    [Tooltip("仮想カーソルの幅と高さ")]
    [SerializeField, Min(1f)] private float cursorSize = 20f;

    [InspectorName("Cursor Color")]
    [Tooltip("仮想カーソルの色")]
    [SerializeField] private Color cursorColor = Color.white;

    /// <summary>
    /// 十字キーによるカーソル移動速度を返す
    /// </summary>
    public float DpadSpeed => dpadSpeed;

    /// <summary>
    /// スティックによるカーソル移動速度を返す
    /// </summary>
    public float StickSpeed => stickSpeed;

    /// <summary>
    /// 仮想カーソルの大きさを返す
    /// </summary>
    public float CursorSize => cursorSize;

    /// <summary>
    /// 仮想カーソルの色を返す
    /// </summary>
    public Color CursorColor => cursorColor;
}
