using UnityEngine;

public abstract class PickupTriggerItem : PoolableObject
{
    private bool pickedUp = false;
  

    private void OnEnable()
    {
        pickedUp = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp)
            return;

        Player player = other.GetComponent<Player>();

        if (player == null)
            return;

        pickedUp = true;

        // Œp³æ‚Å’è‹`‚·‚éˆ—
        OnPickup(player);

        Release();
    }
    private void Update()
    {
        
    }

    protected abstract void OnPickup(Player player);
}