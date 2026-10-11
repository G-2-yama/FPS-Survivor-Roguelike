using Unity.VisualScripting;
using UnityEngine;

public class Effecter : PoolableObject, ILifetimeOwner
{
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private ParticleSystem effectParticleSystem;

    public int LifetimeIndex { get; set; } = -1;

    public override void OnGet()
    {
        LifetimeRunner.Register(this, lifetime);
        effectParticleSystem.Play();
    }

    public void OnLifetimeExpired()
    {
        Release();
    }

    private void OnDisable()
    {
        LifetimeRunner.Unregister(this);
    }
}