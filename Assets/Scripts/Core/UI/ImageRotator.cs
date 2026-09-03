using UnityEngine;

public class ImageRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float speedVariation = 0.5f;
    [SerializeField] private float variationSpeed = 2f;

    [SerializeField] private RectTransform rectTransform;

    private float time;

    private void Update()
    {
        time += Time.deltaTime;

        float variation = Mathf.Sin(time * variationSpeed);

        float currentSpeed = rotationSpeed * (1f + variation * speedVariation);

        rectTransform.Rotate(0f,0f,currentSpeed * Time.deltaTime);
    }
}