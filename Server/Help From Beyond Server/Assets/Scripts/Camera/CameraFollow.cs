using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Speed at which the camera follows the target
    public float followSpeed = 10f;

    // Target to follow
    public Transform m_Target;

    // Offset from the target's position
    private Vector2 offset = Vector2.zero;

    // Original offset value
    public float originalOffset;

    // Flags to lock camera movement on the X or Y axis
    public bool lockX, lockY;

    // Zoom factor for the camera
    [Range(0.125f, 10f)][SerializeField] float zoomFactor = 1.0f;

    // Speed at which the camera zooms
    [SerializeField] float zoomSpeed = 5.0f;

    // Original size of the camera's orthographic view
    private float originalSize = 0f;

    // Reference to the Camera component
    private Camera thisCamera;

    void Start()
    {
        // Get the Camera component and store the original orthographic size
        thisCamera = GetComponent<Camera>();
        originalSize = thisCamera.orthographicSize;
    }

    void LateUpdate()
    {
        // If there is a target to follow
        if (m_Target)
        {
            // Calculate the target position with the offset
            float xTarget = m_Target.position.x + offset.x;
            float yTarget = m_Target.position.y + offset.y;
            float xNew = transform.position.x;

            // If X axis is not locked, interpolate the camera's X position towards the target
            if (!lockX)
                xNew = Mathf.Lerp(transform.position.x, xTarget, Time.deltaTime * followSpeed);

            float yNew = transform.position.y;
            
            // If Y axis is not locked, interpolate the camera's Y position towards the target
            if (!lockY)
                yNew = Mathf.Lerp(transform.position.y, yTarget, Time.deltaTime * followSpeed);

            // Update the camera's position
            transform.position = new Vector3(xNew, yNew, transform.position.z);

            // Calculate the target size for zoom
            float targetSize = originalSize * (1 / zoomFactor);
            
            // If the target size is significantly different from the current size, interpolate towards it
            if (Math.Abs(targetSize - thisCamera.orthographicSize) > 0.01f)
            {
                thisCamera.orthographicSize = Mathf.Lerp(thisCamera.orthographicSize,
                    targetSize, Time.deltaTime * zoomSpeed);
            }
        }
    }

    // Method to increase the Y offset
    public void UpLookOffset()
    {
        offset.y = originalOffset + 2;
    }

    // Method to decrease the Y offset
    public void DownLookOffset()
    {
        offset.y = originalOffset - 2;
    }

    // Method to reset the Y offset to the original value
    public void ResetOffset()
    {
        if (Math.Abs(offset.y - originalOffset) > 0.01)
        {
            offset.y = originalOffset;
        }
    }
}