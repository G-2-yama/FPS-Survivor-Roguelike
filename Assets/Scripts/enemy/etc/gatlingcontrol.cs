using UnityEngine;

public class gatlingcontrol : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float rotationSpeed = 50f;

    void Update()
    {
        // YŽ²Žü‚è‚É‰ñ“]
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
