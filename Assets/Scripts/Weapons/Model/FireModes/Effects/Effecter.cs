using Unity.VisualScripting;
using UnityEngine;

public class Effecter : PoolableObject, ILifetimeOwner
{
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private ParticleSystem effectParticleSystem;
    [Tooltip("プールに戻すとき GameObject を無効化せず、パーティクルを止めて消すだけにする。Collider や DamageArea など、パーティクル以外の部品を持つプレハブでは ON にしない")]
    [SerializeField] private bool keepActiveInPool;

    private ParticleSystem[] particleSystems;

    public int LifetimeIndex { get; set; } = -1;

    public override bool KeepActiveInPool => keepActiveInPool;

    private void Awake()
    {
        if (keepActiveInPool)
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public override void OnGet()
    {
        LifetimeRunner.Register(this, lifetime);

        if (keepActiveInPool)
        {
            foreach (ParticleSystem ps in particleSystems)
                ps.Play(false);
            return;
        }

        effectParticleSystem.Play();
    }

    public override void OnRelease()
    {
        if (!keepActiveInPool)
            return;

        LifetimeRunner.Unregister(this);

        foreach (ParticleSystem ps in particleSystems)
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
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
