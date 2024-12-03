using System;
using System.Collections;
using UnityEngine;


public class WizardMovement : MonoBehaviour
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


    private void Start()
    {
        _cameraShake = FindObjectOfType<CameraShake>();
        dashTimer = gameObject.AddComponent<MyStopwatch>();
        _wizardValues = GetComponent<WizardValues>();
        _inputs = GetComponentInParent<MyInputManager>();
    }


    private void FixedUpdate()
    {
        if (jumping && _inputs.WizardJumpPressed() && _wizardValues.rigidBody.velocity.y > 0)
        {
            _wizardValues.rigidBody.AddForce(Vector2.up * _wizardValues.jumpOnAirForce);
        }
        else
        {
            jumping = false;
        }

        //move on ground
        Vector2 moveDirection = MoveDirectionVectorNormalized();

        //reset the dash
        if (_wizardValues.IsGrounded() && dashPerformed && dashTimer.GetElapsedSeconds() > _wizardValues.dashCooldown)
        {
            ResetDash();
        }

        //reset the double jump
        if (_wizardValues.IsGrounded() && _wizardValues.doubleJumpPerformed)
        {
            ResetDoubleJump();
        }

        //movement
        if (MoveDirectionVectorNormalized() != Vector2.zero)
        {
            //move speed on gruound

            if (_wizardValues.IsGrounded())
            {
                speed = _wizardValues.moveSpeed;
            }
            else if (_wizardValues.rigidBody.velocity.y > 0)
            {
                speed = _wizardValues.moveSpeed / 2;
            }

            //move speed on air
            else
            {
                speed = _wizardValues.moveSpeed / 4;
            }

            moveDirection *= _gamePadAddedSpeed;
            _wizardValues.rigidBody.AddForce(moveDirection * speed -
                                             _wizardValues.rigidBody.velocity);
        }


        UpdateAnimationValues();
    }

    private void Update()
    {
        //jump
        if (_inputs.WizardJumpPerformedThisFrame() && _wizardValues.IsGrounded())
        {
            Jump();
            _wizardValues._playerManager._soundManager.PlayJumpSound();
        }

        //dash
        if (_inputs.WizardDashPerformedThisFrame() && !dashPerformed)
        {
            Dash();
        }

        //double jump
        PlayerManager otherPlayer = _wizardValues._playerManager.GetOtherPlayer();
        if (_inputs.WizardJumpPerformedThisFrame() && !_wizardValues.IsGrounded() && otherPlayer)
        {
            float dist = Vector3.Distance(otherPlayer.transform.position, transform.position);
            if (!_wizardValues.doubleJumpPerformed && dist < _wizardValues.minDistanceToGhost)
            {
                _wizardValues.doubleJumpPerformed = true;
                _wizardValues.rigidBody.velocity *= new Vector2(1, 0);
                ResetDash();
                Jump();
                _wizardValues._playerManager._soundManager.PlayDoubleJumpSound();
            }
        }
    }


    private Vector2 MoveDirectionVectorNormalized()
    {
        Vector2 direction = _inputs.WizardMovement();
        _gamePadAddedSpeed = direction.magnitude;
        if (direction.y > 0.9f)
        {
            _wizardValues._playerManager.cameraFollow.UpLookOffset();
        }
        else if (direction.y < -0.9f)
        {
            _wizardValues._playerManager.cameraFollow.DownLookOffset();
        }
        else
        {
            _wizardValues._playerManager.cameraFollow.ResetOffset();
            if (direction.x > 0)
            {
                _wizardValues.facingDirection = 1;
                _wizardValues.WizardSpriteRenderer.flipX = false;
                _wizardValues.collider2D.offset =
                    new Vector2(Math.Abs(_wizardValues.collider2D.offset.x), _wizardValues.collider2D.offset.y);

                return new Vector2(1, 0);
            }

            if (direction.x < 0)
            {
                _wizardValues.facingDirection = -1;
                _wizardValues.WizardSpriteRenderer.flipX = true;
                _wizardValues.collider2D.offset =
                    new Vector2(-Math.Abs(_wizardValues.collider2D.offset.x), _wizardValues.collider2D.offset.y);
                return new Vector2(-1, 0);
            }
        }


        return Vector2.zero;
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

    private void Jump()
    {
        _wizardValues.rigidBody.velocity *= new Vector2(1, 0);
        if (MoveDirectionVectorNormalized() == Vector2.zero)
        {
            _wizardValues.rigidBody.AddForce(Vector2.up * _wizardValues.jumpForce / 1.25f, ForceMode2D.Impulse);
        }
        else
        {
            _wizardValues.rigidBody.AddForce(Vector2.up * _wizardValues.jumpForce, ForceMode2D.Impulse);
        }

        jumping = true;
    }

    private void Dash()
    {
        _cameraShake.Shake(0.1f, 0.1f);

        _wizardValues.rigidBody.velocity *= new Vector2(0, 1);
        dashTimer.Restart();
        dashPerformed = true;
        _wizardValues.rigidBody.AddForce(new Vector2(_wizardValues.facingDirection, 0) * _wizardValues.dashForce,
            ForceMode2D.Impulse);
        _wizardValues.animationManager.SetDashing(true);
        StartCoroutine(SlowTheDash());
    }


    IEnumerator SlowTheDash()
    {
        float dragOldValue = _wizardValues.rigidBody.drag;
        yield return new WaitForSeconds(0.3f);
        _wizardValues.animationManager.SetDashing(false);
        if (_wizardValues.IsGrounded())
        {
            _wizardValues.rigidBody.drag = 8f;
            _wizardValues.animationManager.SetDashing(false);
        }

        yield return new WaitForSeconds(0.1f);
        _wizardValues.rigidBody.drag = dragOldValue;
    }

    private void UpdateAnimationValues()
    {
        float verticalVelocity = _wizardValues.rigidBody.velocity.y;
        float horizontalVelocity = Math.Abs(_wizardValues.rigidBody.velocity.x);
        //falling
        if (verticalVelocity < -0.1f)
        {
            _wizardValues.animationManager.SetFalling(true);
            _wizardValues.animationManager.SetJumping(false);
        }
        //jump
        else if (verticalVelocity > 0.1f)
        {
            _wizardValues.animationManager.SetFalling(false);
            _wizardValues.animationManager.SetJumping(true);
        }
        //speed
        else if (_wizardValues.IsGrounded())
        {
            _wizardValues.animationManager.SetFalling(false);
            _wizardValues.animationManager.SetJumping(false);
            _wizardValues.animationManager.SetSpeed(horizontalVelocity);
            if (horizontalVelocity < 0.1f)
            {
                _wizardValues.animationManager.SetJoystickMultiplier(1);
            }
            else
            {
                _wizardValues.animationManager.SetJoystickMultiplier(horizontalVelocity);
            }
        }
    }
}