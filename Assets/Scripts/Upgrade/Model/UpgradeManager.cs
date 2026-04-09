using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private Weapon weapon;
    [SerializeField] private Button[] upgradeButtons;

    public void OnUpgradeButtonClicked(int upgradeIndex)
    {
        Debug.Log($"武器レベルがアップグレードされました");
        weapon.LevelUp();

        // アップグレードUIを閉じる
        HideUpgradeUI();
    }

    /// <summary>
    /// アップグレードUIを表示し、ゲームを一時停止する
    /// </summary>
    public void ShowUpgradeUI()
    {
        upgradeCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f; // ゲームを一時停止
    }

    /// <summary>
    /// アップグレードUIを非表示にし、ゲームを再開する
    /// </summary>
    public void HideUpgradeUI()
    {
        upgradeCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f; // ゲームを再開
    }
}
