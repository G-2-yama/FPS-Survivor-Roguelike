using UnityEngine;

public class GameView : MonoBehaviour
{
    [SerializeField] private Canvas gameEndCanvas;

    private void Start()
    {
        gameEndCanvas.gameObject.SetActive(false);
    }

    public void ShowGameEndCanvas()
    {
        gameEndCanvas.gameObject.SetActive(true);
    }
}