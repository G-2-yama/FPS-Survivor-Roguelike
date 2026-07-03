using UnityEngine;

public class StraightMovement : MovementBase
{
    [SerializeField]
    private float speed = 20f;

    protected override void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}