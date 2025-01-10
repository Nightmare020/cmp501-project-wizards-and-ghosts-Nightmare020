using System;
using UnityEngine;

public class WizardValues : MonoBehaviour
{
    [NonSerialized] public PlayerManager _playerManager;
    public float moveSpeed = 5, jumpForce = 10, jumpOnAirForce = 10, dashForce = 10, throwForce = 7;
    [NonSerialized] public float drag;
    public bool doubleJumpPerformed = false;
    public float dashCooldown = 1;
    public float facingDirection;
    public Rigidbody2D rigidBody;
    public SpriteRenderer WizardSpriteRenderer;
    [NonSerialized] public WizardAnimationManager animationManager;
    [NonSerialized] public BoxCollider2D collider2D;
    [SerializeField] private ContactFilter2D walkeableLayers;
    public float minDistanceToGhost;
    private MyInputManager _inputManager;
    private PauseMenu _pauseMenu;


    //grounded variables
    [SerializeField] private Vector2 groundedBoxSize;
    [SerializeField] private float groundedRayDist = 0;
    [SerializeField] private LayerMask groundLayers;

    //sounds
    private SoundManager _soundManager;

    private void Awake()
    {
        _playerManager = GetComponentInParent<PlayerManager>();
        animationManager = GetComponent<WizardAnimationManager>();
        collider2D = GetComponentInChildren<BoxCollider2D>();
        drag = rigidBody.drag;
        _inputManager = GetComponentInParent<MyInputManager>();
        _pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        if (_inputManager.WizardPausePerformed())
        {
            if (!_pauseMenu.isPaused)
            {
                _pauseMenu.PauseGame(_inputManager, false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * groundedRayDist, groundedBoxSize);
    }

    public bool IsGrounded()
    {
        if (Physics2D.BoxCast(transform.position, groundedBoxSize, 0, -Vector2.up, groundedRayDist, groundLayers))
        {
            return true;
        }

        return false;


        //
        // RaycastHit2D hit2D = Physics2D.Raycast(transform.position,
        //     Vector2.down, groundedRayDist, groundLayers);
        // if (!hit2D)
        // {
        //     hit2D = Physics2D.Raycast(transform.position + new Vector3(groundedRayDistOffset, 0),
        //         Vector2.down, groundedRayDist, groundLayers);
        // }
        //
        // if (!hit2D)
        // {
        //     hit2D = Physics2D.Raycast(transform.position - new Vector3(groundedRayDistOffset, 0),
        //         Vector2.down, groundedRayDist, groundLayers);
        // }
        //
        // return rigidBody.IsTouching(walkeableLayers) && hit2D;
    }

    public void Die()
    {
        _playerManager.Die();
    }

    public void Die(Vector2 pos)
    {
        _playerManager.Die(pos);
    }
}