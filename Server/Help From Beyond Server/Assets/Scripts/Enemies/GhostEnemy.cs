using Enemies;
using Unity.Netcode;
using UnityEngine;

public class GhostEnemy : NetworkBehaviour
{
    // Rigidbody2D component for physics interactions
    private Rigidbody2D _rigidbody2D;

    // SpriteRenderer component for rendering the sprite
    private SpriteRenderer _spriteRenderer;
    
    // Collider2D component for collision detection
    private Collider2D _collider2D;

    // Flag to check if the ghost is dead
    private bool dead = false;

    // Direction and speed of the ghost
    [SerializeField] private Vector2 direction = new Vector2(1, 0);
    [SerializeField] private float speed = 1, normalSpeed = 1;
    
    // Minimum distance to the wizard to trigger certain behaviors
    [SerializeField] private float minDistToWizard = 10;
    
    // Color to indicate anger
    [SerializeField] private Color angerColor;

    // References to wizard and ghost values
    private WizardValues _wizardValues;
    private GhostValues _ghostValues;

    // Reference to the enemy manager
    private EnemyManager _enemyManager;

    void Start()
    {
        // Initialize components
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D.gravityScale = 0;
        _enemyManager = FindObjectOfType<EnemyManager>();
    }

    private void OnDrawGizmos()
    {
        // Draw a red wire sphere to visualize the minimum distance to the wizard
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistToWizard);
    }

    private void FixedUpdate()
    {
        // Only the server should update the ghost's behavior and if the ghost is not dead
        if (!IsServer || dead) return;

        // Update wizard and ghost references every 10 frames
        if (Time.frameCount % 10 == 0)
        {
            if (_wizardValues && !_wizardValues.transform.parent.CompareTag("ActiveWizard"))
            {
                _wizardValues = null;
            }

            if (_ghostValues && !_ghostValues.transform.parent.CompareTag("ActiveGhost"))
            {
                _ghostValues = null;
            }

            if (!_wizardValues || !_ghostValues)
            {
                _wizardValues = GetWizard();
                _ghostValues = GetGhost();
            }
        }

        if (_wizardValues && _ghostValues)
        {
            if (!dead)
            {
                // Update speed based on the wizard's position every 2 frames
                if (Time.frameCount % 2 == 0)
                {
                    float relPos = (transform.position - _wizardValues.transform.position).x;
                    if (!_wizardValues._playerManager.isDead && (_wizardValues.facingDirection * relPos) > 0 &&
                        Vector2.Distance(_wizardValues.transform.position, transform.position) < minDistToWizard)
                    {
                        speed = 0;
                    }
                    else
                    {
                        speed = normalSpeed;
                    }
                }

                // Flip the sprite based on the velocity
                if (_rigidbody2D.velocity.x < 0)
                {
                    _spriteRenderer.flipX = true;
                }
                else
                {
                    _spriteRenderer.flipX = false;
                }

                Vector2 direction = Vector2.zero;
                // Chase the ghost if it is not dead
                if (!_ghostValues._playerManager.isDead)
                {
                    _spriteRenderer.color = Color.white;
                    direction = (_ghostValues.transform.position - transform.position).normalized;
                }
                
                // Chase the wizard if the ghost is dead
                else
                {
                    _spriteRenderer.color = angerColor;
                    direction = (_wizardValues.transform.position - transform.position).normalized;
                }

                // Sync movement direction visuals to clients
                bool flipX = _rigidbody2D.velocity.x < 0;
                bool isAngry = _ghostValues._playerManager.isDead;
                SyncVisualsClientRpc(flipX, isAngry);

                // Apply force to move the ghost
                _rigidbody2D.AddForce(direction * speed - _rigidbody2D.velocity);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Only the server should handle collisions
        if (!IsServer) return;

        // Handle collision with the wizard or ghost
        if (!GetGhost()._playerManager.isDead && other.gameObject.CompareTag("ActiveWizard"))
        {
            Die();
        }
        else if (!GetGhost()._playerManager.isDead && other.gameObject.CompareTag("ActiveGhost"))
        {
            GetGhost()._playerManager.Die();
            Die();
        }
        else if (GetGhost()._playerManager.isDead && other.gameObject.CompareTag("ActiveWizard"))
        {
            GetWizard()._playerManager.Die();
            Die();
        }
    }

    // Method to get the ghost values
    GhostValues GetGhost()
    {
        if (!_ghostValues)
        {
            GameObject ghostObj = GameObject.FindWithTag("ActiveGhost");
            if (ghostObj)
            {
                return ghostObj.GetComponentInChildren<GhostValues>();
            }

            return null;
        }

        return _ghostValues;
    }

    // Method to get the wizard values
    WizardValues GetWizard()
    {
        if (!_wizardValues)
        {
            GameObject wizardObj = GameObject.FindWithTag("ActiveWizard");
            if (wizardObj)
            {
                return wizardObj.GetComponentInChildren<WizardValues>();
            }

            return null;
        }

        return _wizardValues;
    }

    // Method to handle the ghost's death
    public void Die()
    {
        if (!IsServer || dead) return;

        dead = true;

        // Add point to the wizard if the wizard is not dead
        if (_wizardValues != null && _wizardValues._playerManager != null &&
            !_wizardValues._playerManager.isDead)
        {
            ArcadeManager.Instance?.AddWizardPoint();
        }

        // Do the visual part on all clients
        DieClientRpc();

        // Hide the ghost and disable its physics and collision
        _spriteRenderer.color = Color.clear;
        _rigidbody2D.simulated = false;
        _collider2D.enabled = false;

        // Notify the enemy manager
        _enemyManager?.OnEnemyDied(this);
    }

    [ClientRpc]
    private void SyncVisualsClientRpc(bool flipX, bool isAngry)
    {
        // Sync the visuals on all clients
        _spriteRenderer.flipX = flipX;
        _spriteRenderer.color = isAngry ? angerColor : Color.white;
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        // Show "death" visuals on all clients
        _spriteRenderer.color = Color.clear;
        _rigidbody2D.simulated = false;
        _collider2D.enabled = false;
    }

    // Method to activate the ghost
    private void Activate()
    {
        dead = false;
        _spriteRenderer.color = Color.white;
        _rigidbody2D.simulated = true;
        _collider2D.enabled = true;
    }

    private void OnBecameInvisible()
    {
        // Reactivate the ghost when it becomes invisible and is dead
        if (IsServer && dead)
        {
            Activate();
        }
    }
}