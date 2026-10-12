using UnityEngine;

public abstract class PickupTriggerItem : PoolableObject
{
    private bool pickedUp = false;

    internal Rigidbody Rb { get; private set; }
    internal Transform CachedTransform { get; private set; }
    internal int MagnetIndex { get; set; } = -1;
    internal bool Captured { get; set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        CachedTransform = transform;
    }

    private void OnEnable()
    {
        PickupMagnet.Register(this);
    }

    private void OnDisable()
    {
        PickupMagnet.Unregister(this);
    }


    
    public override void OnGet()
    {
        pickedUp = false;
        Captured = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp)
            return;

        if (!other.TryGetComponent(out Player player))
            return;

        pickedUp = true;

        // Œp³æ‚Å’è‹`‚·‚éˆ—
        OnPickup(player);
      

        Release();
    }
    
    protected abstract void OnPickup(Player player);
}