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

        offset = offset.normalized * radius;
        offset = Quaternion.AngleAxis(angularSpeed * Time.deltaTime, Vector3.up) * offset;

        Vector3 nextPos = center.position + offset;
        nextPos.y = Mathf.MoveTowards(self.position.y, center.position.y, angularSpeed);

        self.position = nextPos;
        self.rotation = Quaternion.LookRotation(-1*(center.position - self.position));
    }
}
