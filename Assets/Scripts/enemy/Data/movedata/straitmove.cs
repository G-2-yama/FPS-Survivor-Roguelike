using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Toward")]
public class TowardMovement : MovementPattern
{
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float maxSpeed = 3f;
    public override void Move(
        Rigidbody rb,
        Transform self,
        Transform target,
        Vector3 targetPosition,
        int steps)
    {
      
        Vector3 position = rb.position;
        Vector3 toTarget =
            targetPosition - position;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return;
        // Å‘å‘¬“x§ŒÀ
        Vector3 velocity = rb.linearVelocity;
        bool velocityChanged = false;

        Vector3 direction = toTarget.normalized;
        bool atMaxHeight = position.y >= MaxHeight && direction.y > 0f;
        if (atMaxHeight)
        {
            direction.y = 0f;
            direction.Normalize();
        }

        Vector3 stepVelocityChange = direction * (acceleration * Time.fixedDeltaTime);
        for (int i = 0; i < steps; i++)
        {
            if (i > 0)
            {
                velocity += stepVelocityChange;
                velocityChanged = true;
            }

            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
                velocityChanged = true;
            }

            if (atMaxHeight)
            {
                velocity.y = 0f;
                velocityChanged = true;
            }
        }

        if (velocityChanged)
            rb.linearVelocity = velocity;

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
