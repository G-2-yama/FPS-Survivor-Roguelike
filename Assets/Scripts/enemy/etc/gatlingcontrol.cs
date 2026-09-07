using UnityEngine;

public class gatlingcontrol : EnemyAnimationEffect
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float rotationSpeed = 50f;
    private bool isRotating = false;
    private void OnEnable()
    {
        isRotating = false;

    }


    public override void Play()
    {
        isRotating = true;
    }
    public override void Stop()
    {
        isRotating = false;
    }
    void Update()
    {
        if (isRotating)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}
