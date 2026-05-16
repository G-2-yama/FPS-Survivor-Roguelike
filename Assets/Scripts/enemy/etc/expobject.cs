using UnityEngine;

public class ExpObject : PoolableObject
{
    [SerializeField] private int expAmount = 1;

    public int ExpAmount => expAmount;
}