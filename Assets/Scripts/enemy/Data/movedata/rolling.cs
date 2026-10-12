using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Roll")]
public class RollMovement : MovementPattern
{
    [Min(0f)] public float MoveForce = 10f;
    [Tooltip("転がる速さ（水平方向）の上限")]
    [Min(0f)] public float MaxSpeed = 8f;

    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target,
        Vector3 targetPosition,
        int steps)
    {
        Vector3 dir = targetPosition - rb.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.001f)
            return;

        dir.Normalize();

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.sqrMagnitude > MaxSpeed * MaxSpeed)
        {
            horizontal = horizontal.normalized * MaxSpeed;
            rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
        }

        Vector3 torqueAxis =
            Vector3.Cross(Vector3.up, dir);

        rb.AddTorque(
            torqueAxis * MoveForce * steps,
            ForceMode.Acceleration);

        rb.AddForce(
            dir * MoveForce * 0.7f * steps,
            ForceMode.Acceleration);
    }
}