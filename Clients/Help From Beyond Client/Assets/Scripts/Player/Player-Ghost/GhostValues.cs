using System;
using UnityEngine;

public class GhostValues : MonoBehaviour
{
    [NonSerialized] public PlayerManager _playerManager; // Reference to the PlayerManager component
    private MyInputManager _inputManager; // Reference to the MyInputManager component
    public float moveSpeed = 5, throwForce = 7, grabRange = 0.75f, trampolineSpeed = 5; // Movement and action parameters
    public Vector2 aimDirection = new Vector2(1, 0); // Direction the ghost is aiming
    public float facing; // Direction the ghost is facing
    public Rigidbody2D rigidBody; // Reference to the Rigidbody2D component
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    [NonSerialized] public GhostAnimationManager animationManager; // Reference to the GhostAnimationManager component
    [NonSerialized] public new BoxCollider2D collider2D; // Reference to the BoxCollider2D component
    private PauseMenu _pauseMenu; // Reference to the PauseMenu component

    private void Awake()
    {
        // Initialize references to components
        _inputManager = GetComponentInParent<MyInputManager>();
        _playerManager = GetComponentInParent<PlayerManager>();
        animationManager = GetComponent<GhostAnimationManager>();
        collider2D = GetComponent<BoxCollider2D>();
        rigidBody.gravityScale = 0; // Disable gravity for the ghost
        _pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        // Check if the pause button is pressed
        if (_inputManager.GhostPausePerformed())
        {
            if (!_pauseMenu.isPaused)
            {
                _pauseMenu.PauseGame(_inputManager, true);
            }
        }
    }

    // Method to handle the ghost's death
    public void Die()
    {
        _playerManager.Die();
    }

    // Method to handle the ghost's death with a specified position
    public void Die(Vector2 pos)
    {
        _playerManager.Die(pos);
    }
}