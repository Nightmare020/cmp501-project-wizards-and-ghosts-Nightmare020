using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Trampoline : MonoBehaviour
{
    public enum Orientation
    {
        Up,
        Down,
        Left,
        Right,
        Default
    }

    public Orientation _orientation = Orientation.Default;
    private float _trampolineForce = 500;
    private bool _placed;
    public Vector2 _aimDirection, placePosition;
    private MyStopwatch _stopwatch;
    [SerializeField] private float lifeSpawn = 10f;

    private Vector2 _trampolineShotSize = new Vector2(0.75f, 0.75f);

    [SerializeField] private SpriteRenderer _spriteRendererSmall, _spriteRendererBig;
    [FormerlySerializedAs("_boxCollider")] [SerializeField] private Collider2D _collider;
    [SerializeField] private LayerMask collisionLayers;
    private Bullet _bullet;
    [SerializeField] private float minBulletDist = 3;
    private Vector2 tNormal;
    [SerializeField] private LayerMask trampolineLayer;

    private void Start()
    {
        InitComponents();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, _aimDirection, 100, collisionLayers);
        tNormal = hit.normal;
        placePosition = hit.point;

        Vector2 normal = new Vector2(Mathf.RoundToInt(hit.normal.x), Mathf.RoundToInt(hit.normal.y));
        switch (normal)
        {
            case Vector2 v when v.Equals(Vector2.up):
                _orientation = Orientation.Up;
                break;
            case Vector2 v when v.Equals(Vector2.down):
                _orientation = Orientation.Down;
                break;
            case Vector2 v when v.Equals(Vector2.right):
                _orientation = Orientation.Right;
                break;
            case Vector2 v when v.Equals(Vector2.left):
                _orientation = Orientation.Left;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, minBulletDist);
    }

    private void Update()
    {
        if (_stopwatch && _stopwatch.GetElapsedSeconds() > lifeSpawn)
        {
            Destroy(gameObject);
        }

        if (Vector2.Distance(_bullet.transform.position, transform.position) < minBulletDist)
        {
            Vector2 dir = (transform.position - _bullet.transform.position).normalized;
            _bullet.Bounce(dir, tNormal);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_placed)
        {
            if (collision.CompareTag("InstanceItem"))
            {
                Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();
                rb.velocity = Vector2.zero;
                switch (_orientation)
                {
                    case Orientation.Up:
                        rb.AddForce(Vector2.up * _trampolineForce);
                        break;
                    case Orientation.Down:
                        rb.AddForce(Vector2.down * _trampolineForce);
                        break;
                    case Orientation.Left:
                        rb.AddForce(Vector2.left * _trampolineForce);
                        break;
                    case Orientation.Right:
                        rb.AddForce(Vector2.right * _trampolineForce);
                        break;
                }
            }

            // else if (collision.transform.root.CompareTag("Player"))
            // {
            //     Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            //     switch (_orientation)
            //     {
            //         case Orientation.Up:
            //             rb.AddForce(Vector2.up * _trampolineForce);
            //             break;
            //         case Orientation.Down:
            //             rb.AddForce(Vector2.down * _trampolineForce);
            //             break;
            //         case Orientation.Left:
            //             rb.AddForce(Vector2.left * _trampolineForce);
            //             break;
            //         case Orientation.Right:
            //             rb.AddForce(Vector2.right * _trampolineForce);
            //             break;
            //     }
            // }
        }
        else
        {
            if (collision.gameObject.layer == 3 || collision.gameObject.layer == 13)
            {
                PlaceTrampoline();
            }
            else if (collision.gameObject.layer == 20)
            {
                Destroy(gameObject);
                Destroy(collision.gameObject);
            }
        }
    }

    private void InitComponents()
    {
        _bullet = GameObject.FindWithTag("ActiveWizard").GetComponentInChildren<WizardShooter>()._bullet;
        _stopwatch = gameObject.AddComponent<MyStopwatch>();
        _stopwatch.StartStopwatch();
        _spriteRendererSmall.color = Color.white;
        _spriteRendererBig.color = Color.clear;
    }


    private void PlaceTrampoline()
    {
        // _boxCollider.size = new Vector2(0.2f, 0.8f);
        CircleCollider2D collider2D = _collider as CircleCollider2D;
        collider2D.radius = minBulletDist / 2;
        transform.position = placePosition;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        // _collider.isTrigger = false;
        _placed = true;
        switch (_orientation)
        {
            case Orientation.Up:
                transform.rotation = Quaternion.Euler(0, 0, 90);

                break;
            case Orientation.Down:
                transform.rotation = Quaternion.Euler(0, 0, -90);

                break;
            case Orientation.Left:
                transform.rotation = Quaternion.Euler(0, 0, 180);

                break;
            case Orientation.Right:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Orientation.Default:
                break;
            default:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }

        gameObject.layer = 20;
        _spriteRendererSmall.color = Color.clear;
        _spriteRendererBig.color = Color.white;
    }
}