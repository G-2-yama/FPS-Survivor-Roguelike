using UnityEngine;


[CreateAssetMenu(menuName = "Items/healthUp")]
public class HealthUpItem : Item
{
    [SerializeField] private int HealthIncreaseAmount = 200;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
        player.Health.IncreaseHP(HealthIncreaseAmount);
    }
}
