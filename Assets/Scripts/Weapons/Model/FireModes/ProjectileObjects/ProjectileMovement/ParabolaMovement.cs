using UnityEngine;

public class ParabolaMovement : MovementBase
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float upwardSpeed = 8f;

    private Vector3 velocity;

    public override void Initialize(Transform owner, Vector3 direction)
    {
        base.Initialize(owner, direction);
        velocity = direction * speed;
        velocity.y = upwardSpeed;
    }

    protected override void Update()
    {
        velocity += Vector3.down * gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
    }
}