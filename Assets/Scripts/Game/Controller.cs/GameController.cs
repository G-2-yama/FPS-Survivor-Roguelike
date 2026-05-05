
using UnityEngine;

public class GameController
{
    public GameManager gameManager;
    public Timer Timer => gameManager.Timer;
    public Player Player => gameManager.Player;
    public GameStateMachine StateMachine => gameManager.GameStateMachine;

    public GameController(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void ReStartTimer()     => gameManager.Timer.RestartTimer();
    public void StartTimer()       => gameManager.Timer.StartTimer();
    public void StopTimer()      => gameManager.Timer.StopTimer();
    public void PauseGame()      => Time.timeScale = 0f;
    public void ResumeGame()     => Time.timeScale = 1f;
    public void ShowUpgradeUI()  => gameManager.UpgradeManager.ShowUpgradeUI();
    public void HideUpgradeUI()  => gameManager.UpgradeManager.HideUpgradeUI();
    public void ShowGameEnd()    => gameManager.GameView.ShowGameEndCanvas();
}