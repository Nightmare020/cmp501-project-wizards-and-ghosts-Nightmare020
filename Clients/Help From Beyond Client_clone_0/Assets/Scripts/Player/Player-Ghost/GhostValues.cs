using System;
using UnityEngine;

public class GhostValues : MonoBehaviour
{
    [NonSerialized] public PlayerManager _playerManager;
    private MyInputManager _inputManager;
    public float moveSpeed = 5, throwForce = 7, grabRange = 0.75f, trampolineSpeed = 5;
    public Vector2 aimDirection = new Vector2(1, 0);
    public float facing;
    public Rigidbody2D rigidBody;
    public SpriteRenderer spriteRenderer;
    [NonSerialized] public GhostAnimationManager animationManager;
    [NonSerialized] public BoxCollider2D collider2D;
    private PauseMenu _pauseMenu;

    private void Awake()
    {
        _inputManager = GetComponentInParent<MyInputManager>();
        _playerManager = GetComponentInParent<PlayerManager>();
        animationManager = GetComponent<GhostAnimationManager>();
        collider2D = GetComponent<BoxCollider2D>();
        rigidBody.gravityScale = 0;
        _pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Update()
    {
        if (_inputManager.GhostPausePerformed())
        {
            if (!_pauseMenu.isPaused)
            {
                _pauseMenu.PauseGame(_inputManager, true);
            }
        }
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