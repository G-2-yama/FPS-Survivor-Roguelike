using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Toward")]
public class TowardMovement : MovementPattern
{
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float maxSpeed = 3f;
    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target)
    {
      
        Vector3 toTarget =
            target.position - rb.position;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return;
        // Å‘å‘¬“x§ŒÀ
        Vector3 velocity = rb.linearVelocity;

        if (velocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                velocity.normalized * maxSpeed;
        }


        Vector3 direction = toTarget.normalized;
        if (rb.position.y >= MaxHeight && direction.y > 0f)
        {
            velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;
            direction.y = 0f;
            direction.Normalize();
        }

        rb.AddForce(
            direction * acceleration,
            ForceMode.Acceleration);

       
        Quaternion rot =
            Quaternion.LookRotation(-toTarget);

        rb.MoveRotation(rot);
    }
    /*Vector3 toTarget = target.position - rb.position;

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

    rb.MoveRotation(rot);*/
}
