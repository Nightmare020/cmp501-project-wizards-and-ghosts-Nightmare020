using System;

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //offset from the viewport center to fix damping
    public float followSpeed = 10f;
    public Transform m_Target;
    private Vector2 offset = Vector2.zero;
    public float originalOffset;
    public bool lockX, lockY;

    private float margin = 0.1f;

    //zoom
    [Range(0.125f, 10f)] [SerializeField] float zoomFactor = 1.0f;

    [SerializeField] float zoomSpeed = 5.0f;

    private float originalSize = 0f;

    private Camera thisCamera;

    //add line system 

    void Start()
    {
        thisCamera = GetComponent<Camera>();
        originalSize = thisCamera.orthographicSize;
    }


    void LateUpdate()
    {
        if (m_Target)
        {
            //position
            float xTarget = m_Target.position.x + offset.x;
            float yTarget = m_Target.position.y + offset.y;
            float xNew = transform.position.x;
            if (!lockX)
                xNew = Mathf.Lerp(transform.position.x, xTarget, Time.deltaTime * followSpeed);

            float yNew = transform.position.y;
            if (!lockY)
                yNew = Mathf.Lerp(transform.position.y, yTarget, Time.deltaTime * followSpeed);

            transform.position = new Vector3(xNew, yNew, transform.position.z);

            //zoom
            float targetSize = originalSize * (1 / zoomFactor);
            if (Math.Abs(targetSize - thisCamera.orthographicSize) > 0.01f)
            {
                thisCamera.orthographicSize = Mathf.Lerp(thisCamera.orthographicSize,
                    targetSize, Time.deltaTime * zoomSpeed);
            }
        }
    }


    public void UpLookOffset()
    {
        offset.y = originalOffset + 2;
    }

    public void DownLookOffset()
    {
        offset.y = originalOffset - 2;
    }

    public void ResetOffset()
    {
        if (Math.Abs(offset.y - originalOffset) > 0.01)
        {
            offset.y = originalOffset;
        }
    }

    void SetZoom(float zoomFactor)
    {
        this.zoomFactor = zoomFactor;
    }
}