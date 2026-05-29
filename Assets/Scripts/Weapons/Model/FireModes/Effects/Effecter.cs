using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Effecter : PoolableObject
{
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private ParticleSystem effectParticleSystem;

    private Coroutine lifeRoutine;

    public override void OnGet()
    {
        lifeRoutine = StartCoroutine(LifeTimer());
        effectParticleSystem.Play();
    }

    public override void OnRelease()
    {
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Release();
    }
}