using UnityEngine;

public class PlayerMover
{
    private const float StopInputDeadzoneSqr = 0.0001f;
    private const float GroundedVerticalVelocity = -2f;

    private CharacterController characterController;
    private Transform playerTransform;
    private PlayerConfig config;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    public PlayerMover(
        Transform playerTransform,
        CharacterController characterController,
        PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.characterController = characterController;
        this.config = config;
    }

    public void Move(Vector2 moveInput, bool run, float deltaTime)
    {
        float speed = run ? config.RunSpeed : config.WalkSpeed;
        Vector3 move = playerTransform.right * moveInput.x + playerTransform.forward * moveInput.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        Vector3 targetHorizontalVelocity = move * speed;

        if (move.sqrMagnitude <= StopInputDeadzoneSqr)
        {
            horizontalVelocity = Vector3.zero;
        }
        else
        {
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontalVelocity,
                config.GroundAcceleration * deltaTime
            );
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
        }

        verticalVelocity += config.Gravity * deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(velocity * deltaTime);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
        }
    }

    public void Jump()
    {
        verticalVelocity = config.JumpForce;
    }

    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    public void ResolveWallHit(Vector3 wallNormal)
    {
        if (wallNormal.y > 0.1f)
        {
            return;
        }

        horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
    }

    public void Stop()
    {
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }
}
