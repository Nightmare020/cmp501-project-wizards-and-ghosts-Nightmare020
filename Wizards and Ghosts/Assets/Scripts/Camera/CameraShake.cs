
using UnityEngine;

public class CameraShake : MonoBehaviour
{
// Transform of the camera to shake. Grabs the gameObject's transform
// if null.

// How long the object should shake for.
    public float shakeDuration = 0f;

// Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.1f;

    Vector3 originalPos;

    public void Shake(float duration,float amount)
    {
        shakeDuration = duration;
        shakeAmount = amount;
    }

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void OnEnable()
    {
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.position += Random.insideUnitSphere * shakeAmount;
			
            shakeDuration -= Time.deltaTime;
        }
       
    }
}