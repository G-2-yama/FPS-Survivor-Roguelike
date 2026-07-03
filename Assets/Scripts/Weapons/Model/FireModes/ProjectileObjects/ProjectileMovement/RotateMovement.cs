using UnityEngine;

public class RotateMovement : MovementBase
{
    [SerializeField]
    private float speed = 20f;

    protected override void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}