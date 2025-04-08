using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GhostMovement : NetworkBehaviour
{
    private GhostValues _ghostValues;
    private float _gamePadAddedSpeed;
    private MyInputManager _inputs;

    private Vector2 _cachedInput;
    private Vector2 _smoothedInput = Vector2.zero;

    private Vector2 _screenBounds;
    private Vector2 _upperBound;
    private Vector2 _lowerBound;
    private float _objectWidth;
    private float _objectHeight;

    private Vector3 lastServerPosition;
    private Vector2 lastServerVelocity;
    private const float reconciliationThreshold = 0.1f;

    private struct InputFrame
    {
        public float timestamp;
        public Vector2 input;
        public float strength;
    }

    private Queue<InputFrame> inputHistory = new Queue<InputFrame>();

    private void Start()
    {
        _ghostValues = GetComponent<GhostValues>();
        _inputs = GetComponentInParent<MyInputManager>();

        _objectWidth = _ghostValues.spriteRenderer.bounds.extents.x; //extents = size of width / 2
        _objectHeight = _ghostValues.spriteRenderer.bounds.extents.y; //extents = size of height / 2

        _screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            SimulateMovement(_cachedInput, _gamePadAddedSpeed);
        }
        else if (IsOwner)
        {
            Vector2 input = _inputs.GhostMovement();
            float inputStrength = input.magnitude;
            _cachedInput = input;
            _gamePadAddedSpeed = inputStrength;

            float timestamp = Time.time;

            // Client prediction
            SimulateMovement(input, inputStrength);
            UpdateFacingDirection(input);

            inputHistory.Enqueue(new InputFrame
            {
                timestamp = timestamp,
                input = input,
                strength = inputStrength
            });

            if (inputHistory.Count > 100)
                inputHistory.Dequeue();

            SendInputToServerRpc(input, inputStrength, timestamp);

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

        SimulateMovement(input, strength);
        SendReconciliationClientRpc(transform.position, _ghostValues.rigidBody.velocity, timestamp);
        UpdateGhostVisualClientRpc(input);
    }

    [ClientRpc]
    private void SendReconciliationClientRpc(Vector3 position, Vector2 velocity, float timestamp)
    {
        if (!IsOwner) return;

        lastServerPosition = position;
        lastServerVelocity = velocity;

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
        float heigth = _objectHeight;

        viewPos.x = Mathf.Clamp(viewPos.x, bounds.x + width, bounds.z - width);
        viewPos.y = Mathf.Clamp(viewPos.y, bounds.y + heigth, bounds.w - heigth);

        transform.root.position = viewPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(_upperBound.x, _upperBound.y, 0), 1);
        Gizmos.DrawSphere(new Vector3(_lowerBound.x, _lowerBound.y, 0), 1);
    }
}
