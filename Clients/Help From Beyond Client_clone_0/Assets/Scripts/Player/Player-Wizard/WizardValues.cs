using System;
using UnityEngine;

public class WizardValues : MonoBehaviour
{
    [NonSerialized] public PlayerManager _playerManager; // Reference to the PlayerManager component
    public float moveSpeed = 5, jumpForce = 10, jumpOnAirForce = 10, dashForce = 10, throwForce = 7; // Movement and action parameters
    [NonSerialized] public float drag; // Drag value for the Rigidbody2D
    public bool doubleJumpPerformed = false; // Flag to check if double jump is performed
    public float dashCooldown = 1; // Cooldown time for dashing
    public float facingDirection; // Direction the wizard is facing
    public Rigidbody2D rigidBody; // Reference to the Rigidbody2D component
    public SpriteRenderer WizardSpriteRenderer; // Reference to the SpriteRenderer component
    [NonSerialized] public WizardAnimationManager animationManager; // Reference to the WizardAnimationManager component
    [NonSerialized] public new BoxCollider2D collider2D; // Reference to the BoxCollider2D component
    [SerializeField] private ContactFilter2D walkeableLayers; // Contact filter for walkable layers
    public float minDistanceToGhost; // Minimum distance to the ghost
    private MyInputManager _inputManager; // Reference to the MyInputManager component
    private PauseMenu _pauseMenu; // Reference to the PauseMenu component

    // Grounded variables
    [SerializeField] private Vector2 groundedBoxSize; // Size of the box for checking if grounded
    [SerializeField] private float groundedRayDist = 0; // Distance for the grounded raycast
    [SerializeField] private LayerMask groundLayers; // Layer mask for ground layers

    // Sounds
    private SoundManager _soundManager; // Reference to the SoundManager component

    private void Awake()
    {
        // Initialize references to components
        _playerManager = GetComponentInParent<PlayerManager>();
        animationManager = GetComponent<WizardAnimationManager>();
        collider2D = GetComponentInChildren<BoxCollider2D>();
        drag = rigidBody.drag;
        _inputManager = GetComponentInParent<MyInputManager>();
        _pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        // Check if the pause button is pressed
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
        // Draw a wire cube to visualize the grounded box
        Gizmos.DrawWireCube(transform.position - transform.up * groundedRayDist, groundedBoxSize);
    }

    public bool IsGrounded()
    {
        // Check if the wizard is grounded using a box cast
        if (Physics2D.BoxCast(transform.position, groundedBoxSize, 0, -Vector2.up, groundedRayDist, groundLayers))
        {
            return true;
        }

        return false;
    }

    // Method to handle the wizard's death
    public void Die()
    {
        _playerManager.Die();
    }

    // Method to handle the wizard's death with a specified position
    public void Die(Vector2 pos)
    {
        _playerManager.Die(pos);
    }
}