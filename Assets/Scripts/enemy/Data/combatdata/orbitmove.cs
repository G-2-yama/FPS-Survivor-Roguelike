using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Orbit")]
public class OrbitMovement : MovementPattern
{
    public float OrbitRadius = 3f;
    public float OrbitAngularSpeed = 180f;

    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target)
    {
        Vector3 offset = rb.position - target.position;

        if (offset.sqrMagnitude <= 0.0001f)
        {
            offset = -target.forward * OrbitRadius;
        }

        float currentRadius = offset.magnitude;
        float radiusDiff = OrbitRadius - currentRadius;

        offset += offset.normalized * radiusDiff * 0.1f;

        offset =
            Quaternion.AngleAxis(
                OrbitAngularSpeed * Time.fixedDeltaTime,
                Vector3.up)
            * offset;

        Vector3 nextPos = target.position + offset;

        rb.MovePosition(nextPos);

        Vector3 toCenter =
            target.position - rb.position;

        rb.MoveRotation(
            Quaternion.LookRotation(-toCenter));
    }
}