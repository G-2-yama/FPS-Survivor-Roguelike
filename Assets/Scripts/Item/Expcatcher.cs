using UnityEngine;
[CreateAssetMenu(menuName = "Items/Expcatcher")]
public class Expcatcher : Item
{

    [SerializeField] private EnemyConfig expdata;
    [SerializeField] private float increaseamount = 5;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
        expdata.IncreaseDistance(increaseamount);
    }

    public override void Revert()
    {
       expdata.IncreaseDistance(-increaseamount);
    }
}
