using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // How long the object should shake for
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder
    public float shakeAmount = 0.1f;

    // Original position of the camera
    Vector3 originalPos;

    // Method to initiate the shake with a specified duration and amount
    public void Shake(float duration, float amount)
    {
        shakeDuration = duration;
        shakeAmount = amount;
    }

    void Awake()
    {
        // Store the original position of the camera
        originalPos = transform.localPosition;
    }

    void Update()
    {
        // If there is shake duration left
        if (shakeDuration > 0)
        {
            // Apply a random shake to the camera's position
            transform.position += Random.insideUnitSphere * shakeAmount;

            // Decrease the shake duration over time
            shakeDuration -= Time.deltaTime;
        }
    }
}