using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/item")]
public class ItemMovement : MovementPattern
{
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float maxSpeed = 3f;
    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target)
    {

        Vector3 toTarget = target.position - rb.position;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return;

        Vector3 move =
            toTarget.normalized *
            ChaseSpeed *
            Time.fixedDeltaTime;

        Vector3 nextPos = rb.position + move;
        nextPos.y = Mathf.Min(nextPos.y, MaxHeight);

        rb.MovePosition(nextPos);

        Quaternion rot =
            Quaternion.LookRotation(-toTarget);

        rb.MoveRotation(rot);
    }
  
}
