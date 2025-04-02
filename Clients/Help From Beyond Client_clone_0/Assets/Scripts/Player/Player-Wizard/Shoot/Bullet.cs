using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private CircleCollider2D _collider2D;
    //private EnemyManager _enemyManager;
    private static readonly int Shoot1 = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("die");
    public bool isBeingUsed = false;
    public bool enhanced = false;
    private CameraShake _cameraShake;
    private bool hasBeenDespawned = false;

    private Vector3 origin;
    //bullet particles

    //shoot particles
    [SerializeField] private Transform shootEffect;
    [SerializeField] private Animator shootAnimator;

    //impact particles
    [SerializeField] private Transform impactEffect;
    [SerializeField] private Animator impactAnimator;

    //bounce particles
    [SerializeField] private Transform bounceEffect;
    [SerializeField] private Animator bounceAnimator;

    //sound
    [SerializeField] private AudioClip shootSound, BounceSound, ImpactSound;
    [SerializeField] private AudioSource _audioSource;

    //children
    private List<Transform> children;

    private void Awake()
    {
        children = new List<Transform>();
        children.AddRange(GetComponentsInChildren<Transform>());
    }

    private void Start()
    {
        _cameraShake = FindObjectOfType<CameraShake>();

        //_enemyManager = FindObjectOfType<EnemyManager>();
        shootEffect.parent = null;
        impactEffect.parent = null;
        bounceEffect.parent = null;
    }

    private void Update()
    {
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
        _collider2D.enabled = true;
        _sprite.enabled = true;
    }

    public void DisableBullet()
    {
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
        shootEffect.position = ray.GetPoint(0.5f);
        shootAnimator.SetTrigger("Shoot");

        // Avoid playing audio on the sever if it's not a host client
        if (!IsServer || IsHost)
        {
            _audioSource.clip = shootSound;
            _audioSource.Play();
        }

        yield return new WaitForSeconds(0.5f);

        EnableBullet();
        transform.position = ray.GetPoint(0.6f);
        _rigidbody2D.velocity = ray.direction * speed;
        transform.right = ray.direction;

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
            // play impact effect
            Impact(transform.position);
        }
    }

    public void Bounce(Vector2 direction, Vector2 normal)
    {
        //sound
        _audioSource.clip = BounceSound;
        _audioSource.Play();
        //camer shake
        _cameraShake.Shake(0.1f, 0.1f);
        //effect
        bounceEffect.position = transform.position;
        bounceAnimator.SetTrigger("Bounce");
        ChangeLayer(6);
        float speed = _rigidbody2D.velocity.magnitude;
        Vector2 reflectDirection = Vector2.Reflect(direction, normal).normalized;

        //if (_enemyManager)
        //{
        //    Transform enemyPos = _enemyManager.GetClosestWizzardEnemy(transform.position);
        //    Transform enemyGhost = _enemyManager.GetClosestGhostEnemy(transform.position);

        //    if (enemyPos)
        //    {
        //        reflectDirection = (enemyPos.position - transform.position).normalized;
        //    }
        //    else if (enemyGhost)
        //    {
        //        reflectDirection = (enemyGhost.position - transform.position).normalized;
        //    }
        //}

        transform.right = reflectDirection;
        _rigidbody2D.velocity = reflectDirection * speed;
        _sprite.color = Color.blue;
        enhanced = true;
        //play bounce effects
    }

    public void Impact(Vector2 colpoint)
    {
        if (hasBeenDespawned) return;

        hasBeenDespawned = true;
        _audioSource.clip = ImpactSound;
        _audioSource.Play();

        //impact animation
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
            // remove from network
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
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //trampolin
        if (!other.gameObject.CompareTag("Wizard"))
        {
            //enemies
            if (other.gameObject.CompareTag("Ghost Enemy"))
            {
                GhostEnemy enemy = other.gameObject.GetComponent<GhostEnemy>();
                if (enhanced)
                {
                    enemy.Die();
                }

                Impact(transform.position);
            }
            //scene
            else
            {
                Impact(transform.position);
            }
        }
    }

    private void ChangeLayer(int layer)
    {
        foreach (var child in children)
        {
            if (child != null)
            {
                child.gameObject.layer = layer;
            }
        }
    }
}