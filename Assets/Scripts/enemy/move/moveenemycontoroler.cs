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
}