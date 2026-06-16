using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Tracer : PoolableObject
{
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private TrailRenderer trailRenderer;

    private Coroutine lifeRoutine;

    public override void OnGet()
    {
        lifeRoutine = StartCoroutine(LifeTimer());
    }

    public override void OnRelease()
    {
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }
    }

    public void Initialize(Vector3 startPos, Vector3 endPos, float trailTime)
    {
        trailRenderer.Clear();
        trailRenderer.emitting = false;
        trailRenderer.transform.position = startPos;
        trailRenderer.time = trailTime;

        StartCoroutine(SpawnTracer(startPos, endPos));
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Release();
    }

    private IEnumerator SpawnTracer(Vector3 startPos, Vector3 endPos)
    {
        float time = 0;

        yield return null;
        trailRenderer.emitting = true;

        while (time < 1f)
        {
            trailRenderer.transform.position =
                Vector3.Lerp(startPos, endPos, time);

            time += Time.deltaTime / trailRenderer.time;

            yield return null;
        }

        trailRenderer.transform.position = endPos;
    }
}