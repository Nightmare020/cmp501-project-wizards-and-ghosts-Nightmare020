using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Limits the camera movement to follow the ghost player within certain bounds.
/// </summary>
public class GhostCameraLimiter : MonoBehaviour
{
    // Reference to the CameraFollow component
    private CameraFollow _cameraFollow;

    // Reference to the main camera
    private Camera _mainCamera;

    // Transform of the ghost player to follow
    private Transform _ghostTarget;

    // Reference to the ghost player manager
    private PlayerManager _ghostPlayer;

    // Reference to the wizard player manager
    private PlayerManager _wizardPlayer;

    // Flag to determine if the camera should follow the ghost
    private bool cameraShouldFollow = true;

    public void Initialize(PlayerManager ghostPlayer)
    {
        // Get the main camera
        _mainCamera = Camera.main;

        // Get the CameraFollow component from the main camera
        _cameraFollow = _mainCamera.GetComponent<CameraFollow>();

        // Set the ghost player and its transform
        _ghostPlayer = ghostPlayer;
        _ghostTarget = ghostPlayer.transform;

        // Set the camera follow target to the ghost player
        _cameraFollow.m_Target = _ghostTarget;

        // Enable camera following
        cameraShouldFollow = true;
        enabled = true;
    }

    private void LateUpdate()
    {
        // If the ghost player or its target transform is null, return early
        if (_ghostPlayer == null || _ghostTarget == null) return;

        // Get the wizard player associated with the ghost player
        _wizardPlayer = _ghostPlayer.GetOtherPlayer();

        // If the wizard player is null or not in the Wizard state, return early
        if (_wizardPlayer == null ||
            _wizardPlayer.currentState.Value != PlayerState.Wizard) return;

        // Get the camera bounds from the wizard player
        Vector4 bounds = _wizardPlayer.cameraBounds.Value;

        // Calculate the desired camera position if it followed the ghost
        Vector3 desiredCamPos = new Vector3(
            _ghostTarget.position.x,
            _ghostTarget.position.y,
            _mainCamera.transform.position.z
        );

        // Check if the desired camera position is within the bounds
        bool wouldBeInside =
            desiredCamPos.x >= bounds.x &&
            desiredCamPos.x <= bounds.z &&
            desiredCamPos.y >= bounds.y &&
            desiredCamPos.y <= bounds.w;

        // If the desired position is within bounds, set the camera to follow the ghost
        if (wouldBeInside)
        {
            if (!cameraShouldFollow)
            {
                _cameraFollow.m_Target = _ghostTarget;
                cameraShouldFollow = true;
            }
        }
        // If the desired position is outside bounds, stop the camera from following the ghost
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