using UnityEngine;

public abstract class MovementBase : MonoBehaviour
{
    protected Transform owner;
    protected Vector3 direction;

    public virtual void Initialize(Transform owner, Vector3 direction)
    {
        this.owner = owner;
        this.direction = direction.normalized;
    }

    protected virtual void Update()
    {
    }
}