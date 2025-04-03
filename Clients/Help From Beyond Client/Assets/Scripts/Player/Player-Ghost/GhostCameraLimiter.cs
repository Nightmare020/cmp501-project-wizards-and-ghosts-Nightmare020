using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GhostCameraLimiter : MonoBehaviour
{
    private CameraFollow _cameraFollow;
    private Camera _mainCamera;
    private Transform _ghostTarget;

    private PlayerManager _ghostPlayer;
    private PlayerManager _wizardPlayer;

    private bool cameraShouldFollow = true;

    // Start is called before the first frame update
    public void Initialize(PlayerManager ghostPlayer)
    {
        _mainCamera = Camera.main;
        _cameraFollow = _mainCamera.GetComponent<CameraFollow>();

        _ghostPlayer = ghostPlayer;
        _ghostTarget = ghostPlayer.transform;
        _cameraFollow.m_Target = _ghostTarget;

        cameraShouldFollow = true;
        enabled = true;
    }

    private void LateUpdate()
    {
        if (_ghostPlayer == null || _ghostTarget == null) return;

        _wizardPlayer = _ghostPlayer.GetOtherPlayer();

        if (_wizardPlayer == null || 
            _wizardPlayer.currentState.Value != PlayerState.Wizard) return;

        Vector4 bounds = _wizardPlayer.cameraBounds.Value;

        // Calculate where the camera would move to if it followed the ghost
        Vector3 desiredCamPos = new Vector3(
            _ghostTarget.position.x,
            _ghostTarget.position.y,
            _mainCamera.transform.position.z
            );

        bool wouldBeInside =
            desiredCamPos.x >= bounds.x &&
            desiredCamPos.x <= bounds.z &&
            desiredCamPos.y >= bounds.y &&
            desiredCamPos.y <= bounds.w;

        if (wouldBeInside)
        {
            if (!cameraShouldFollow)
            {
                _cameraFollow.m_Target = _ghostTarget;
                cameraShouldFollow = true;
            }
        }
        else
        {
            if (cameraShouldFollow)
            {
                _cameraFollow.m_Target = null;
                cameraShouldFollow = false;
            }
        }
    }
}
