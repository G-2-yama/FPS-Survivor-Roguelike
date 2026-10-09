using UnityEngine;

[CreateAssetMenu(fileName = "GameModeData", menuName = "Game/Game Mode Data")]
public class GameModeData : ScriptableObject
{
    [Tooltip("モード選択画面に表示する名前")]
    [SerializeField] private string displayName = "New Mode";

    [Tooltip("制限時間（分） 負の値（-1など）でエンドレス")]
    [SerializeField] private float timeLimitMinutes = 10f;

    public string DisplayName => displayName;
    public float TimeLimitSeconds => timeLimitMinutes * 60f;
}
