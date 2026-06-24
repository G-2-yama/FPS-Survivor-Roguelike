using UnityEngine;

public class Expcatcher : Item
{

    [SerializeField] private EnemyConfig expdata;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
    }

    public override void Revert()
    {
       
    }
}
