using UnityEngine;

[CreateAssetMenu(fileName = "GameModeData", menuName = "Game/Game Mode Data")]
public class GameModeData : ScriptableObject
{
    [Tooltip("モード選択画面に表示する名前")]
    [SerializeField] private string displayName = "New Mode";

    public string DisplayName => displayName;
}
