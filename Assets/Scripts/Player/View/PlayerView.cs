using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Text currentHealthText;

    private void Start()
    {
        player.Health.OnHealthChanged += UpdateHealthText;
    }

    private void UpdateHealthText(int current, int max)
    {
        currentHealthText.text = $"{current} / {max}";
    }

}