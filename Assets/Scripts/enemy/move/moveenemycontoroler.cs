using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public void MoveTowards(Transform self, Vector3 destination, float moveSpeed)
    {
        Vector3 toTarget = destination - self.position;
        if (toTarget.sqrMagnitude <= 0.0001f)
            return;

        self.position += toTarget.normalized * moveSpeed * Time.deltaTime;
        self.rotation = Quaternion.LookRotation(-toTarget);
    }

    public void OrbitAround(Transform self, Transform center, float radius, float angularSpeed)
    {
        Vector3 offset = self.position - center.position;

        if (offset.sqrMagnitude <= 0.0001f)
        {
            offset = -center.forward * radius;
        }

       
        float currentRadius = offset.magnitude;
        float targetRadius = radius;
        float radiusDiff = targetRadius - currentRadius;

        // ”¼Œa‚ð‚ä‚é‚­•â³iŒW”‚Í’²®ƒ|ƒCƒ“ƒgj
        offset += offset.normalized * radiusDiff * 0.1f;
        // ============================

        // ‰ñ“]
        offset = Quaternion.AngleAxis(angularSpeed * Time.deltaTime, Vector3.up) * offset;

        Vector3 nextPos = center.position + offset;

        // Y•â³
        nextPos.y = Mathf.MoveTowards(self.position.y, center.position.y, 1 * Time.deltaTime);

        self.position = nextPos;

        // Œü‚«
        Vector3 toCenter = center.position - self.position;
        self.rotation = Quaternion.LookRotation(-toCenter);
    }
}
