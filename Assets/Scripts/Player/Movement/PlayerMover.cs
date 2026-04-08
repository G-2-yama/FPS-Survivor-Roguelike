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
    private bool wasGrounded;
    private float remainingJumpHoldBonus;
    private float jumpHoldTimer;

    public PlayerMover(
        Transform playerTransform,
        CharacterController characterController,
        PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.characterController = characterController;
        this.config = config;
    }

    public void Move(Vector2 moveInput, bool run, bool jumpHeld, float deltaTime)
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && !wasGrounded)
        {
            horizontalVelocity = Vector3.zero;
            remainingJumpHoldBonus = 0f;
            jumpHoldTimer = 0f;
        }

        float speed = run ? config.RunSpeed : config.WalkSpeed;
        Vector3 move = playerTransform.right * moveInput.x + playerTransform.forward * moveInput.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        Vector3 targetHorizontalVelocity = move * speed;

        if (move.sqrMagnitude <= StopInputDeadzoneSqr)
        {
            if (isGrounded)
            {
                horizontalVelocity = Vector3.zero;
            }
        }
        else
        {
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontalVelocity,
                config.GroundAcceleration * deltaTime
            );
        }

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
        }

        if (!isGrounded && jumpHeld && verticalVelocity > 0f && jumpHoldTimer > 0f && remainingJumpHoldBonus > 0f)
        {
            float holdBonusPerSecond = config.JumpForce * (config.JumpHoldMaxMultiplier - 1f)
                / config.JumpHoldDuration;
            float appliedBonus = Mathf.Min(holdBonusPerSecond * deltaTime, remainingJumpHoldBonus);
            verticalVelocity += appliedBonus;
            remainingJumpHoldBonus -= appliedBonus;
            jumpHoldTimer = Mathf.Max(0f, jumpHoldTimer - deltaTime);
        }

        verticalVelocity += config.Gravity * deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(velocity * deltaTime);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
        }

        wasGrounded = characterController.isGrounded;
    }

    public void Jump()
    {
        verticalVelocity = config.JumpForce;
        remainingJumpHoldBonus = config.JumpForce * (config.JumpHoldMaxMultiplier - 1f);
        jumpHoldTimer = config.JumpHoldDuration;
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
        wasGrounded = false;
        remainingJumpHoldBonus = 0f;
        jumpHoldTimer = 0f;
    }
}
