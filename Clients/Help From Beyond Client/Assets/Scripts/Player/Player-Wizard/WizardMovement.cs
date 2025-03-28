using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;


public class WizardMovement : NetworkBehaviour
{
    private WizardValues _wizardValues;
    private float _gamePadAddedSpeed;
    private MyInputManager _inputs;
    private float speed = 0;
    private CameraShake _cameraShake;

    // actions performed
    private bool dashPerformed = false;

    //jump things
    private bool jumping;
    private bool falling;

    //dash timer
    private MyStopwatch dashTimer;

    private Vector2 cachedInput;

    private void Start()
    {
        _cameraShake = FindObjectOfType<CameraShake>();
        dashTimer = gameObject.AddComponent<MyStopwatch>();
        _wizardValues = GetComponent<WizardValues>();
        _inputs = GetComponentInParent<MyInputManager>();
    }


    private void FixedUpdate()
    {
        if (!IsServer) return;

        HandleServerMovement();
        UpdateAnimationValues(); // Server updates the animations
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleClienInput();
    }

    // ================================
    // CLIENT: Input Collection
    // ================================
    private void HandleClienInput()
    {
        // Movement input
        cachedInput = _inputs.WizardMovement();
        SendMovementInputServerRpc(cachedInput);

        // Jump
        if (_inputs.WizardJumpPerformedThisFrame())
        {
            SendJumpRequestServerRpc();
        }

        // Dash
        if (_inputs.WizardDashPerformedThisFrame())
        {
            SendDashRequestServerRpc();
        }
    }

    // ================================
    // SERVER RPCs (Called by Owner Client)
    // ================================
    [ServerRpc]
    private void SendMovementInputServerRpc(Vector2 input)
    {
        cachedInput = input;
    }

    [ServerRpc]
    private void SendJumpRequestServerRpc()
    {
        TryJump();
    }

    [ServerRpc]
    private void SendDashRequestServerRpc()
    {
        TryDash();
    }

    // ================================
    // SERVER: Movement + Logic
    // ================================
    private void HandleServerMovement()
    {
        Vector2 direction = cachedInput.normalized;
        _gamePadAddedSpeed = direction.magnitude;
        if (direction == Vector2.zero) return;

        float moveSpeed = _wizardValues.IsGrounded() ? _wizardValues.moveSpeed 
            : _wizardValues.rigidBody.velocity.y > 0 ? _wizardValues.moveSpeed / 2:
        _wizardValues.moveSpeed / 4;

        direction *= _gamePadAddedSpeed;
        Vector2 force = direction * moveSpeed - _wizardValues.rigidBody.velocity;
        _wizardValues.rigidBody.AddForce(force);

        UpdateFacingDirection(direction);

        // Dash cooldown
        if (_wizardValues.IsGrounded() && dashPerformed && dashTimer.GetElapsedSeconds() > _wizardValues.dashCooldown)
        {
            ResetDash();
        }

        // Double jump reset
        if (_wizardValues.IsGrounded() && _wizardValues.doubleJumpPerformed)
        {
            ResetDoubleJump();
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
    }

    private void Jump()
    {
        _wizardValues.rigidBody.velocity *= new Vector2(1, 0);

        float force = cachedInput == Vector2.zero 
            ? _wizardValues.jumpForce / 1.25f 
            : _wizardValues.jumpForce;

        _wizardValues.rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        jumping = true;
    }

    private void ResetDoubleJump()
    {
        _wizardValues.doubleJumpPerformed = false;
    }

    private void TryDash()
    {
        if (dashPerformed || !_wizardValues.IsGrounded()) return;

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

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            _wizardValues.facingDirection = 1;
            _wizardValues.WizardSpriteRenderer.flipX = false;
            _wizardValues.collider2D.offset =
                new Vector2(Mathf.Abs(_wizardValues.collider2D.offset.x), _wizardValues.collider2D.offset.y);

            // Set the direction of the player to the right
            SetFacingClientRpc(1);
        }
        else if (direction.x < 0)
        {
            _wizardValues.facingDirection = -1;
            _wizardValues.WizardSpriteRenderer.flipX = true;
            _wizardValues.collider2D.offset =
                new Vector2(-Math.Abs(_wizardValues.collider2D.offset.x), _wizardValues.collider2D.offset.y);

            // Set the direction of the player to the left
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

        //Speed
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
        if (IsServer) return;

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
        if (IsServer) return;

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
        if (IsServer) return;

        _wizardValues.animationManager.SetDashing(isDashing);
    }
}