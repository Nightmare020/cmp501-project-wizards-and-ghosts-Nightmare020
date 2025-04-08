using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D; // Reference to the Rigidbody2D component
    [SerializeField] private SpriteRenderer _sprite; // Reference to the SpriteRenderer component
    [SerializeField] private CircleCollider2D _collider2D; // Reference to the CircleCollider2D component
    private static readonly int Shoot1 = Animator.StringToHash("Shoot"); // Hash for the "Shoot" animation
    private static readonly int Die = Animator.StringToHash("die"); // Hash for the "die" animation
    public bool isBeingUsed = false; // Flag to check if the bullet is being used
    public bool enhanced = false; // Flag to check if the bullet is enhanced
    private CameraShake _cameraShake; // Reference to the CameraShake component
    private bool hasBeenDespawned = false; // Flag to check if the bullet has been despawned

    private Vector3 origin; // Origin position of the bullet

    // Bullet particles
    [SerializeField] private Transform shootEffect; // Transform for the shoot effect
    [SerializeField] private Animator shootAnimator; // Animator for the shoot effect

    [SerializeField] private Transform impactEffect; // Transform for the impact effect
    [SerializeField] private Animator impactAnimator; // Animator for the impact effect

    [SerializeField] private Transform bounceEffect; // Transform for the bounce effect
    [SerializeField] private Animator bounceAnimator; // Animator for the bounce effect

    // Sound
    [SerializeField] private AudioClip shootSound, BounceSound, ImpactSound; // Audio clips for the bullet sounds
    [SerializeField] private AudioSource _audioSource; // Audio source for playing sounds

    // Children
    private List<Transform> children; // List of child transforms

    private void Awake()
    {
        // Initialize the list of child transforms
        children = new List<Transform>();
        children.AddRange(GetComponentsInChildren<Transform>());
    }

    private void Start()
    {
        // Find the CameraShake component in the scene
        _cameraShake = FindObjectOfType<CameraShake>();

        if (IsServer)
        {
            // Detach the particle effects from the bullet
            shootEffect.parent = null;
            impactEffect.parent = null;
            bounceEffect.parent = null;
        }
    }

    private void Update()
    {
        // Check if the bullet has traveled too far from its origin
        if (Time.frameCount % 10 == 0 && isBeingUsed)
        {
            if (Vector2.Distance(transform.position, origin) > 50)
            {
                Impact(transform.position);
            }
        }
    }

    public void EnableBullet()
    {
        // Enable the collider and sprite renderer
        _collider2D.enabled = true;
        _sprite.enabled = true;
    }

    public void DisableBullet()
    {
        // Disable the collider and sprite renderer
        _collider2D.enabled = false;
        _sprite.enabled = false;
    }

    public void Shoot(Ray ray, float speed)
    {
        origin = ray.origin;
        isBeingUsed = true;
        enhanced = false;
        _sprite.color = Color.white;

        ChangeLayer(14);
        StartCoroutine(ShootCoroutine(ray, speed));

        // Despawn after 3 seconds
        StartCoroutine(DestroyAfterSeconds(3f));
    }

    IEnumerator ShootCoroutine(Ray ray, float speed)
    {
        // Disable visual + collider immediately
        DisableBullet();

        // Play charge-up VFX
        shootEffect.position = ray.GetPoint(0.5f);
        shootAnimator.SetTrigger("Shoot");

        // Avoid playing audio on the server if it's not a host client
        if (!IsServer || IsHost)
        {
            _audioSource.clip = shootSound;
            _audioSource.Play();
        }

        // Wait for charge-up effect
        yield return new WaitForSeconds(0.5f);

        // Now launch the bullet visually + physically
        EnableBullet();

        if (IsServer)
        {
            transform.position = ray.GetPoint(0.6f);
            transform.right = ray.direction;
            _rigidbody2D.velocity = ray.direction * speed;
        }

        if (!IsServer || IsHost)
        {
            _cameraShake.Shake(0.1f, 0.1f);
        }
    }

    private IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!hasBeenDespawned)
        {
            // Play impact effect
            Impact(transform.position);
        }
    }

    public void Bounce(Vector2 direction, Vector2 normal)
    {
        // Play bounce sound
        _audioSource.clip = BounceSound;
        _audioSource.Play();
        // Camera shake
        _cameraShake.Shake(0.1f, 0.1f);
        // Play bounce effect
        bounceEffect.position = transform.position;
        bounceAnimator.SetTrigger("Bounce");
        ChangeLayer(6);
        float speed = _rigidbody2D.velocity.magnitude;
        Vector2 reflectDirection = Vector2.Reflect(direction, normal).normalized;

        transform.right = reflectDirection;
        _rigidbody2D.velocity = reflectDirection * speed;
        _sprite.color = Color.blue;
        enhanced = true;
    }

    public void Impact(Vector2 colpoint)
    {
        if (hasBeenDespawned) return;

        hasBeenDespawned = true;
        _audioSource.clip = ImpactSound;
        _audioSource.Play();

        // Play impact animation
        impactEffect.position = colpoint;
        impactAnimator.SetTrigger("Impact");

        _rigidbody2D.velocity = Vector2.zero;

        // Call reset after short delay to allow VFX to play
        StartCoroutine(ResetAfterVFX());
    }

    private IEnumerator ResetAfterVFX()
    {
        yield return new WaitForSeconds(0.2f);
        ResetBullet();

        if (IsServer && NetworkObject.IsSpawned)
        {
            // Remove from network
            NetworkObject.Despawn(true);
        }

        Destroy(gameObject);
    }

    private void ResetBullet()
    {
        // Re-parent VFX back to this bullet
        shootEffect.SetParent(transform);
        impactEffect.SetParent(transform);
        bounceEffect.SetParent(transform);

        // Disable sprite + collider
        DisableBullet();
        isBeingUsed = false;
        enhanced = false;
    }

    public void PlayShootVFX(Vector3 effectPos)
    {
        // Hide visuals + collider during buildup
        DisableBullet();

        if (shootEffect != null) shootEffect.position = effectPos;
        if (shootAnimator != null) shootAnimator.SetTrigger("Shoot");

        if (_audioSource != null && shootSound != null)
        {
            _audioSource.clip = shootSound;
            _audioSource.Play();
        }

        if (_cameraShake != null)
        {
            _cameraShake.Shake(0.1f, 0.1f);
        }

        // Then enable visuals slightly after (purely visuals, won't shoot)
        StartCoroutine(RevealClientBulletVisuals());
    }

    private IEnumerator RevealClientBulletVisuals()
    {
        yield return new WaitForSeconds(0.5f);
        EnableBullet();
    }

    public void SetClientTransform(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.right = direction;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Check if the bullet collided with a non-wizard object
        if (!other.gameObject.CompareTag("Wizard"))
        {
            // Check if the bullet collided with a ghost enemy
            if (other.gameObject.CompareTag("Ghost Enemy"))
            {
                GhostEnemy enemy = other.gameObject.GetComponent<GhostEnemy>();
                if (enhanced)
                {
                    enemy.Die();
                }

                Impact(transform.position);
            }
            // Handle collision with other objects
            else
            {
                Impact(transform.position);
            }
        }
    }

    private void ChangeLayer(int layer)
    {
        // Change the layer of all child objects
        foreach (var child in children)
        {
            if (child != null)
            {
                child.gameObject.layer = layer;
            }
        }
    }
}