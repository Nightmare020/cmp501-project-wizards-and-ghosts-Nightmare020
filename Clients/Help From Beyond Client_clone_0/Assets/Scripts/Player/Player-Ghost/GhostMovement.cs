using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GhostMovement : NetworkBehaviour
{
    private GhostValues _ghostValues; // Reference to GhostValues component
    private float _gamePadAddedSpeed; // Speed added by gamepad input
    private MyInputManager _inputs; // Reference to input manager

    private Vector2 _cachedInput; // Cached input for movement
    private Vector2 _smoothedInput = Vector2.zero; // Smoothed input for movement

    private Vector2 _screenBounds; // Screen bounds for clamping position
    private Vector2 _upperBound; // Upper bound for movement
    private Vector2 _lowerBound; // Lower bound for movement
    private float _objectWidth; // Width of the ghost object
    private float _objectHeight; // Height of the ghost object

    private Vector3 lastServerPosition; // Last known position from the server
    private Vector2 lastServerVelocity; // Last known velocity from the server
    private const float reconciliationThreshold = 0.1f; // Threshold for position reconciliation

    private struct InputFrame
    {
        public float timestamp; // Timestamp of the input
        public Vector2 input; // Input vector
        public float strength; // Strength of the input
    }

    private Queue<InputFrame> inputHistory = new Queue<InputFrame>(); // Queue to store input history

    private void Start()
    {
        _ghostValues = GetComponent<GhostValues>();
        _inputs = GetComponentInParent<MyInputManager>();

        _objectWidth = _ghostValues.spriteRenderer.bounds.extents.x; // Extents = size of width / 2
        _objectHeight = _ghostValues.spriteRenderer.bounds.extents.y; // Extents = size of height / 2

        _screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            // Simulate movement on the server
            SimulateMovement(_cachedInput, _gamePadAddedSpeed);
        }
        else if (IsOwner)
        {
            // Get input from the player
            Vector2 input = _inputs.GhostMovement();
            float inputStrength = input.magnitude;
            _cachedInput = input;
            _gamePadAddedSpeed = inputStrength;

            float timestamp = Time.time;

            // Client prediction
            SimulateMovement(input, inputStrength);
            UpdateFacingDirection(input);

            // Store input in history
            inputHistory.Enqueue(new InputFrame
            {
                timestamp = timestamp,
                input = input,
                strength = inputStrength
            });

            // Limit the size of the input history
            if (inputHistory.Count > 100)
                inputHistory.Dequeue();

            // Send input to the server
            SendInputToServerRpc(input, inputStrength, timestamp);

            // Reconcile position with the server
            float error = Vector3.Distance(transform.position, lastServerPosition);
            if (error > reconciliationThreshold)
            {
                transform.position = Vector3.Lerp(transform.position, lastServerPosition, 0.5f);
                _ghostValues.rigidBody.velocity = Vector2.Lerp(_ghostValues.rigidBody.velocity, lastServerVelocity, 0.5f);
            }
        }
    }

    private void SimulateMovement(Vector2 direction, float strength)
    {
        if (direction == Vector2.zero)
        {
            _ghostValues.rigidBody.velocity = Vector2.zero;
            return;
        }

        float speed = _ghostValues.moveSpeed;
        Vector2 desiredVelocity = direction.normalized * strength * speed;
        _ghostValues.rigidBody.velocity = desiredVelocity;
    }

    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, float strength, float timestamp)
    {
        _cachedInput = input;
        _gamePadAddedSpeed = strength;

        // Simulate movement on the server
        SimulateMovement(input, strength);

        // Send reconciliation data to the client
        SendReconciliationClientRpc(transform.position, _ghostValues.rigidBody.velocity, timestamp);

        // Update ghost visuals on the client
        UpdateGhostVisualClientRpc(input);
    }

    [ClientRpc]
    private void SendReconciliationClientRpc(Vector3 position, Vector2 velocity, float timestamp)
    {
        if (!IsOwner) return;

        lastServerPosition = position;
        lastServerVelocity = velocity;

        // Remove old inputs from the history
        while (inputHistory.Count > 0 && inputHistory.Peek().timestamp <= timestamp)
        {
            inputHistory.Dequeue();
        }
    }

    private void UpdateFacingDirection(Vector2 input)
    {
        if (input.x > 0)
        {
            _ghostValues.spriteRenderer.flipX = false;
            _ghostValues.aimDirection.x = 1;
            _ghostValues.facing = 1;
        }
        else if (input.x < 0)
        {
            _ghostValues.spriteRenderer.flipX = true;
            _ghostValues.aimDirection.x = -1;
            _ghostValues.facing = -1;
        }

        _ghostValues.aimDirection.y = input.y == 0 ? 0 : (input.y > 0 ? 1 : -1);
    }

    [ClientRpc]
    private void UpdateGhostVisualClientRpc(Vector2 input)
    {
        if (IsOwner || !Application.isPlaying) return;
        UpdateFacingDirection(input);
    }

    void LateUpdate()
    {
        var wizard = _ghostValues._playerManager.GetOtherPlayer();
        if (wizard == null || wizard.cameraFollow == null) return;

        Vector4 bounds = wizard.cameraBounds.Value;
        if (bounds == Vector4.zero) return;

        Vector3 viewPos = transform.root.position;
        float width = _objectWidth;
        float height = _objectHeight;

        // Clamp the position within the camera bounds
        viewPos.x = Mathf.Clamp(viewPos.x, bounds.x + width, bounds.z - width);
        viewPos.y = Mathf.Clamp(viewPos.y, bounds.y + height, bounds.w - height);

        transform.root.position = viewPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(_upperBound.x, _upperBound.y, 0), 1);
        Gizmos.DrawSphere(new Vector3(_lowerBound.x, _lowerBound.y, 0), 1);
    }
}