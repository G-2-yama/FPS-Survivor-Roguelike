using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BoostFireCuller : MonoBehaviour
{
    [Tooltip("カメラからこの距離より遠いと放出を止める（m）")]
    [SerializeField] private float cullDistance = 40f;

    private ParticleSystem particle;
    private bool emitting;
    private bool applied;

    public int RunnerIndex { get; set; } = -1;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        applied = false;
        BoostFireCullerRunner.Register(this);
    }

    private void OnDisable()
    {
        BoostFireCullerRunner.Unregister(this);
    }

    public void Refresh(Vector3 cameraPosition)
    {
        float sqrDistance = (transform.position - cameraPosition).sqrMagnitude;
        bool shouldEmit = sqrDistance <= cullDistance * cullDistance;
        if (applied && shouldEmit == emitting)
            return;

        applied = true;
        emitting = shouldEmit;
        if (shouldEmit)
            particle.Play(true);
        else
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
