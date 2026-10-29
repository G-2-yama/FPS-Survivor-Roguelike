using UnityEngine;

[CreateAssetMenu(fileName = "GameModeData", menuName = "Game/Game Mode Data")]
public class GameModeData : ScriptableObject
{
    [Tooltip("モード選択画面に表示する名前")]
    [SerializeField] private string displayName = "New Mode";

    [Tooltip("制限時間（分） 負の値（-1など）でエンドレス")]
    [SerializeField] private float timeLimitMinutes = 10f;

    [Tooltip("予定表の時刻に広場の出口を塞ぐ閉じ込めを起こすか")]
    [SerializeField] private bool lockdownEnabled = true;

    public string DisplayName => displayName;
    public float TimeLimitSeconds => timeLimitMinutes * 60f;
    public bool LockdownEnabled => lockdownEnabled;
}
