using UnityEngine;

public class OrbitAmmoMovement : MovementBase
{
    [SerializeField] private float radius;
    [SerializeField] private float speed;
    private float angle;

    protected override void Update()
    {
        angle += speed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),0,Mathf.Sin(angle * Mathf.Deg2Rad))* radius;

        transform.position = owner.position + offset;
    }
}