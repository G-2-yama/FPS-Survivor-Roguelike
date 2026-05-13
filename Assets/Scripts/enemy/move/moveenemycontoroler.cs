using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public void MoveTowards(Transform self, Vector3 destination, float moveSpeed)
    {
        Rigidbody rb = self.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 toTarget = destination - rb.position;
        if (toTarget.sqrMagnitude <= 0.0001f)
            return;

        Vector3 move = toTarget.normalized * moveSpeed * Time.fixedDeltaTime;
        Vector3 nextPos = rb.position + move;

        rb.MovePosition(nextPos);

        Quaternion rot = Quaternion.LookRotation(-toTarget);
        rb.MoveRotation(rot);
    }


    public void OrbitAround(Transform self, Transform center, float radius, float angularSpeed)
    {
        Rigidbody rb = self.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 offset = rb.position - center.position;

        if (offset.sqrMagnitude <= 0.0001f)
        {
            offset = -center.forward * radius;
        }

        float currentRadius = offset.magnitude;
        float radiusDiff = radius - currentRadius;

        // ”¼Œa•â³
        offset += offset.normalized * radiusDiff * 0.1f;

        // ‰ñ“]
        offset = Quaternion.AngleAxis(angularSpeed * Time.fixedDeltaTime, Vector3.up) * offset;

        Vector3 nextPos = center.position + offset;

        // Y•â³
        nextPos.y = Mathf.MoveTowards(rb.position.y, center.position.y, 1 * Time.fixedDeltaTime);

        rb.MovePosition(nextPos);

        // Œü‚«
        Vector3 toCenter = center.position - rb.position;
        Quaternion rot = Quaternion.LookRotation(-toCenter);
        rb.MoveRotation(rot);
    }
    public void RollTowards(
    Transform self,
    Vector3 destination,
    float moveForce)
    {
        Rigidbody rb = self.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 dir = destination - rb.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.001f)
            return;

        dir.Normalize();

        Vector3 torqueAxis = Vector3.Cross(Vector3.up, dir);

        rb.AddTorque(
            torqueAxis * moveForce,
            ForceMode.Acceleration);
        rb.AddForce(
        dir * moveForce * 0.7f,
        ForceMode.Acceleration);

    }
    public void JumpToward(
    Transform self,
    Transform target,
    float forwardForce,
    float upwardForce)
    {
        Rigidbody rb = self.GetComponent<Rigidbody>();
        if (rb == null) return;

        // ˜A‘±ƒWƒƒƒ“ƒv–hŽ~
        if (rb.linearVelocity.y > 0.1f)
            return;

        Vector3 dir =
            (target.position - self.position).normalized;

        Vector3 force =
            dir * forwardForce +
            Vector3.up * upwardForce;

        rb.AddForce(force, ForceMode.Impulse);
    }
}