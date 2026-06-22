using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Roll")]
public class RollMovement : MovementPattern
{
    [Min(0f)] public float MoveForce = 10f;

    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target)
    {
        Vector3 dir = target.position - rb.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.001f)
            return;

        dir.Normalize();

        Vector3 torqueAxis =
            Vector3.Cross(Vector3.up, dir);

        rb.AddTorque(
            torqueAxis * MoveForce,
            ForceMode.Acceleration);

        rb.AddForce(
            dir * MoveForce * 0.7f,
            ForceMode.Acceleration);
    }
}