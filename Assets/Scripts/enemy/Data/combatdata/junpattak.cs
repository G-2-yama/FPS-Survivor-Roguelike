using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Jump")]
public class JumpMovement : MovementPattern
{
    public float ForwardForce = 12f;
    public float UpwardForce = 6f;

    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target)
    {
        if (rb.linearVelocity.y > 0.1f)
            return;

        Vector3 dir =
            (target.position - self.position)
            .normalized;

        Vector3 force =
            dir * ForwardForce +
            Vector3.up * UpwardForce;

        rb.AddForce(force, ForceMode.Impulse);
    }
}