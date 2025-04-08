using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WizardMovement : NetworkBehaviour
{
    private WizardValues _wizardValues; // Reference to WizardValues component
    private float _gamePadAddedSpeed; // Speed added by gamepad input
    private MyInputManager _inputs; // Reference to input manager
    private CameraShake _cameraShake; // Reference to CameraShake component

    // Actions performed
    private bool dashPerformed = false;

    // Dash timer
    private MyStopwatch dashTimer;

    private Vector2 cachedInput; // Cached input for movement

    private struct InputFrame
    {
        public float timestamp; // Timestamp of the input
        public Vector2 movement; // Input vector for movement
        public bool jump; // Flag to check if jump is performed
        public bool dash; // Flag to check if dash is performed
    }

    private Queue<InputFrame> inputHistory = new Queue<InputFrame>(); // Queue to store input history
    private const float reconciliationThreshold = 0.15f; // Threshold for position reconciliation
    private Vector3 lastServerPosition; // Last known position from the server
    private Vector2 lastServerVelocity; // Last known velocity from the server

    private float jumpBufferTimer = 0f; // Timer for jump input buffering
    private float dashBufferTimer = 0f; // Timer for dash input buffering
    private const float inputBufferDuration = 0.25f; // Duration for input buffering

    private bool _isLocallyGrounded; // Flag to check if the wizard is grounded

    private float coyoteTimer = 0f; // Timer for coyote time
    private const float coyoteTime = 0.1f; // Duration for coyote time

    // Store smoothed input
    private Vector2 _smoothedInput = Vector2.zero;

    private void Start()
    {
        // Initialize references to components
        _cameraShake = FindObjectOfType<CameraShake>();
        dashTimer = gameObject.AddComponent<MyStopwatch>();
        _wizardValues = GetComponent<WizardValues>();
        _inputs = GetComponentInParent<MyInputManager>();

        coyoteTimer = coyoteTime;
        jumpBufferTimer = 0f;
        dashBufferTimer = 0f;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Check if jump or dash input is performed
        if (_inputs.WizardJumpPerformedThisFrame())
            jumpBufferTimer = inputBufferDuration;

        if (_inputs.WizardDashPerformedThisFrame())
            dashBufferTimer = inputBufferDuration;
    }

    private void FixedUpdate()
    {
        _isLocallyGrounded = _wizardValues.IsGrounded();
        coyoteTimer = _isLocallyGrounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);

        if (!IsServer && _isLocallyGrounded && dashPerformed &&
            dashTimer.GetElapsedSeconds() > _wizardValues.dashCooldown)
        {
            dashPerformed = false;
        }

        if (IsServer)
        {
            HandleServerMovement();
            UpdateAnimationValues(); // Server updates the animations
        }
        else if (IsOwner)
        {
            Vector2 moveInput = _inputs.WizardMovement();

            jumpBufferTimer -= Time.fixedDeltaTime;
            dashBufferTimer -= Time.fixedDeltaTime;

            bool jump = jumpBufferTimer > 0f;
            bool dash = dashBufferTimer > 0f;

            float timestamp = Time.time;
            cachedInput = moveInput;

            SimulateMovement(moveInput);

            // Predict actions locally only if not the server
            if (IsOwner)
            {
                if (jump) TryJumpClientPredicted();
                if (dash) TryDashClientPredicted();
            }

            inputHistory.Enqueue(new InputFrame
            {
                timestamp = timestamp,
                movement = moveInput,
                jump = jump,
                dash = dash
            });

            // Prevent unbounded growth
            if (inputHistory.Count > 100)
            {
                inputHistory.Dequeue();
            }

            SendInputToServerRpc(moveInput, jump, dash, timestamp);

            if (jump) jumpBufferTimer = 0f;
            if (dash) dashBufferTimer = 0f;

            HandleCameraLookOffset(moveInput);

            float positionError = Vector3.Distance(transform.position, lastServerPosition);
            if (positionError > reconciliationThreshold)
            {
                // Smoothly interpolate back to server position
                float blend = Mathf.Clamp01((positionError - reconciliationThreshold) * 5f);
                transform.position = Vector3.Lerp(transform.position, lastServerPosition, blend);
                _wizardValues.rigidBody.velocity = Vector2.Lerp(_wizardValues.rigidBody.velocity, lastServerVelocity, blend);
            }
        }
    }

    private void SimulateMovement(Vector2 input)
    {
        _smoothedInput = Vector2.Lerp(_smoothedInput, input, 0.25f); // Smooth input over time

        Vector2 velocity = _wizardValues.rigidBody.velocity;
        float moveSpeed = _isLocallyGrounded ? _wizardValues.moveSpeed
            : velocity.y > 0 ? _wizardValues.moveSpeed / 2 : _wizardValues.moveSpeed / 4;

        if (_smoothedInput != Vector2.zero)
        {
            Vector2 dir = _smoothedInput.normalized;
            float targetSpeed = _smoothedInput.magnitude * moveSpeed;
            velocity.x = Mathf.Lerp(velocity.x, dir.x * targetSpeed, 0.3f);
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0f, 0.15f);
        }

        _wizardValues.rigidBody.velocity = new Vector2(
            Mathf.Clamp(velocity.x, -moveSpeed * 1.1f, moveSpeed * 1.1f),
            _wizardValues.rigidBody.velocity.y
        );

        UpdateFacingDirection(_smoothedInput);
    }

    // ================================
    // CLIENT: Reconciliation
    // ================================
    [ClientRpc]
    private void SendReconciliationClientRpc(Vector3 pos, Vector2 velocity, float timestamp)
    {
        if (!IsOwner) return;

        lastServerPosition = pos;
        lastServerVelocity = velocity;

        while (inputHistory.Count > 0 && inputHistory.Peek().timestamp <= timestamp)
        {
            inputHistory.Dequeue();
        }
    }

    // ================================
    // SERVER RPCs (Called by Owner Client)
    // ================================
    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, bool jump, bool dash, float timestamp)
    {
        cachedInput = input;
        HandleServerMovement();

        if (jump) TryJump();
        if (dash) TryDash();

        SendReconciliationClientRpc(transform.position, _wizardValues.rigidBody.velocity, timestamp);
    }

    // ================================
    // SERVER: Movement + Logic
    // ================================
    private void HandleServerMovement()
    {
        Vector2 direction = cachedInput.normalized;
        float strength = direction.magnitude;
        if (direction == Vector2.zero) return;

        float moveSpeed = _wizardValues.IsGrounded() ? _wizardValues.moveSpeed
            : _wizardValues.rigidBody.velocity.y > 0 ? _wizardValues.moveSpeed / 2 :
              _wizardValues.moveSpeed / 4;

        direction *= strength;
        Vector2 force = direction * moveSpeed - _wizardValues.rigidBody.velocity;
        _wizardValues.rigidBody.AddForce(force);

        UpdateFacingDirection(direction);

        if (_wizardValues.IsGrounded() && dashPerformed && dashTimer.GetElapsedSeconds() > _wizardValues.dashCooldown)
        {
            ResetDash();
        }

        if (_wizardValues.IsGrounded() && _wizardValues.doubleJumpPerformed)
        {
            ResetDoubleJump();
        }
    }

    private void HandleCameraLookOffset(Vector2 input)
    {
        if (input.y > 0.9f)
        {
            _wizardValues._playerManager.cameraFollow.UpLookOffset();
        }
        else if (input.y < -0.9f)
        {
            _wizardValues._playerManager.cameraFollow.DownLookOffset();
        }
        else
        {
            _wizardValues._playerManager.cameraFollow.ResetOffset();
        }
    }

    private void TryJump()
    {
        if (_wizardValues.IsGrounded())
        {
            Jump();
            _wizardValues._playerManager._soundManager.PlayJumpSound();
        }
        else
        {
            TryDoubleJump();
        }
    }

    private void TryJumpClientPredicted()
    {
        bool canJump =
        coyoteTimer > 0f ||
        (_wizardValues.rigidBody.velocity.y > -0.01f && _wizardValues.rigidBody.velocity.y < 0.01f);

        if (canJump)
        {
            Vector2 vel = _wizardValues.rigidBody.velocity;
            vel.y = _wizardValues.jumpForce;
            _wizardValues.rigidBody.velocity = vel;
            coyoteTimer = 0f;
        }
    }

    private void TryDoubleJump()
    {
        PlayerManager other = _wizardValues._playerManager.GetOtherPlayer();

        if (other != null && !_wizardValues.doubleJumpPerformed)
        {
            float dist = Vector3.Distance(other.transform.position, transform.position);
            if (dist < _wizardValues.minDistanceToGhost)
            {
                _wizardValues.doubleJumpPerformed = true;
                _wizardValues.rigidBody.velocity *= new Vector2(1, 0);
                ResetDash();
                Jump();
                _wizardValues._playerManager._soundManager.PlayDoubleJumpSound();
            }
        }
    }

    private void Jump()
    {
        _wizardValues.rigidBody.velocity *= new Vector2(1, 0);

        float force = cachedInput == Vector2.zero
            ? _wizardValues.jumpForce / 1.25f
            : _wizardValues.jumpForce;

        _wizardValues.rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void TryDash()
    {
        if (dashPerformed || !_wizardValues.IsGrounded()) return;

        ExecuteDash();
    }

    private void TryDashClientPredicted()
    {
        bool canDash = !dashPerformed && (coyoteTimer > 0f || Mathf.Abs(_wizardValues.rigidBody.velocity.y) < 0.01f);

        if (!canDash) return;

        Vector2 vel = _wizardValues.rigidBody.velocity;
        vel.x = _wizardValues.facingDirection * _wizardValues.dashForce;
        _wizardValues.rigidBody.velocity = vel;
        dashPerformed = true;
    }

    private void ExecuteDash()
    {
        _cameraShake.Shake(0.1f, 0.1f);
        _wizardValues.rigidBody.velocity *= new Vector2(0, 1);

        dashTimer.Restart();
        dashPerformed = true;

        Vector2 dashForce = new Vector2(_wizardValues.facingDirection, 0) * _wizardValues.dashForce;
        _wizardValues.rigidBody.AddForce(dashForce, ForceMode2D.Impulse);

        _wizardValues.animationManager.SetDashing(true);
        SetDashAnimationClientRpc(true);
        StartCoroutine(SlowTheDash());
    }

    IEnumerator SlowTheDash()
    {
        float dragOldValue = _wizardValues.rigidBody.drag;
        yield return new WaitForSeconds(0.3f);
        _wizardValues.animationManager.SetDashing(false);
        SetDashAnimationClientRpc(false);

        if (_wizardValues.IsGrounded())
        {
            _wizardValues.rigidBody.drag = 8f;
        }

        yield return new WaitForSeconds(0.1f);
        _wizardValues.rigidBody.drag = dragOldValue;
    }

    private void ResetDash()
    {
        dashPerformed = false;
        dashTimer.Stop();
        dashTimer.ResetStopwatch();
    }

    private void ResetDoubleJump()
    {
        _wizardValues.doubleJumpPerformed = false;
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            _wizardValues.facingDirection = 1;
            _wizardValues.WizardSpriteRenderer.flipX = false;
            _wizardValues.collider2D.offset =
                new Vector2(Mathf.Abs(_wizardValues.collider2D.offset.x), _wizardValues.collider2D.offset.y);

            SetFacingClientRpc(1);
        }
        else if (direction.x < 0)
        {
            _wizardValues.facingDirection = -1;
            _wizardValues.WizardSpriteRenderer.flipX = true;
            _wizardValues.collider2D.offset =
                new Vector2(-Math.Abs(_wizardValues.collider2D.offset.x), _wizardValues.collider2D.offset.y);

            SetFacingClientRpc(-1);
        }
    }

    // ================================
    // Shared: Animation
    // ================================
    private void UpdateAnimationValues()
    {
        float verticalVelocity = _wizardValues.rigidBody.velocity.y;
        float horizontalVelocity = Math.Abs(_wizardValues.rigidBody.velocity.x);

        // Falling
        if (verticalVelocity < -0.1f)
        {
            _wizardValues.animationManager.SetFalling(true);
            _wizardValues.animationManager.SetJumping(false);
        }

        // Jump
        else if (verticalVelocity > 0.1f)
        {
            _wizardValues.animationManager.SetFalling(false);
            _wizardValues.animationManager.SetJumping(true);
        }

        // Speed
        else if (_wizardValues.IsGrounded())
        {
            _wizardValues.animationManager.SetFalling(false);
            _wizardValues.animationManager.SetJumping(false);
            _wizardValues.animationManager.SetSpeed(horizontalVelocity);
            _wizardValues.animationManager.SetJoystickMultiplier(Mathf.Max(1f, horizontalVelocity));
        }

        UpdateAnimationClientRpc(verticalVelocity, horizontalVelocity, _wizardValues.IsGrounded());
    }

    // ================================
    // CLIENT: Server Replication
    // ================================
    [ClientRpc]
    private void SetFacingClientRpc(int direction)
    {
        // Server already handled
        if (IsServer || _wizardValues == null || _wizardValues.animationManager == null) return;

        _wizardValues.facingDirection = direction;

        bool flip = direction < 0;
        _wizardValues.WizardSpriteRenderer.flipX = flip;

        Vector2 offset = _wizardValues.collider2D.offset;
        offset.x = Mathf.Abs(offset.x) * (flip ? -1 : 1);
        _wizardValues.collider2D.offset = offset;
    }

    [ClientRpc]
    private void UpdateAnimationClientRpc(float verticalVel, float horizontalVel, bool grounded)
    {
        // Already handled in server
        if (IsServer || _wizardValues == null || _wizardValues.animationManager == null)
            return;

        var anim = _wizardValues.animationManager;

        if (verticalVel < -0.1f)
        {
            anim.SetFalling(true);
            anim.SetJumping(false);
        }
        else if (verticalVel > 0.1f)
        {
            anim.SetFalling(false);
            anim.SetJumping(true);
        }
        else if (grounded)
        {
            anim.SetFalling(false);
            anim.SetJumping(false);
            anim.SetSpeed(horizontalVel);
            anim.SetJoystickMultiplier(Mathf.Max(1f, horizontalVel));
        }
    }

    [ClientRpc]
    private void SetDashAnimationClientRpc(bool isDashing)
    {
        // Already handled in server
        if (IsServer || _wizardValues == null || _wizardValues.animationManager == null) return;

        _wizardValues.animationManager.SetDashing(isDashing);
    }
}