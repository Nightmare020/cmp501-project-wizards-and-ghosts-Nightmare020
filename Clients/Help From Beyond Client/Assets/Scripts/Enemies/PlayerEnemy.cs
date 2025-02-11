using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;

    Camera camera;
    private bool dead = false;
    private Vector2 screenBounds;

    [SerializeField] private Vector2 direction = new Vector2(1, 0);
    [SerializeField] private float speed = 1, maxSpeed = 3;
    [SerializeField] private float colDistance = 0.1f, colDistanceSledge = 0.1f, sledgeDistance = 1;
    [SerializeField] private LayerMask collision;
    [SerializeField] private AudioSource _audioSourceInstant;

    void Start()
    {
        camera = Camera.main;
        screenBounds = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.transform.position.z));
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, colDistance);
        // Ray r = new Ray((Vector2)transform.position + direction * sledgeDistance, Vector2.down * colDistanceSledge);
        Gizmos.DrawRay((Vector2)transform.position + direction * sledgeDistance, Vector2.down * colDistanceSledge);
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            _rigidbody2D.AddForce(direction * speed - _rigidbody2D.velocity);
            if (Time.frameCount % 3 == 0)
            {
                RaycastHit2D hit2D = Physics2D.Raycast(transform.position, direction, colDistance, collision);
                RaycastHit2D hit2DSledge = Physics2D.Raycast((Vector2)transform.position + direction * sledgeDistance,
                    Vector2.down,
                    colDistanceSledge, collision);

                if (hit2D || (!hit2DSledge && _rigidbody2D.IsTouchingLayers(collision)))
                {
                    direction *= -1;
                    if (direction.x < 0)
                    {
                        _spriteRenderer.flipX = true;
                    }
                    else
                    {
                        _spriteRenderer.flipX = false;
                    }
                }
            }
        }
    }

    public void Die()
    {
        dead = true;
        _audioSourceInstant.Play();
        _spriteRenderer.color = Color.clear;
        _rigidbody2D.simulated = false;
        _collider2D.enabled = false;
    }


    public void IncreaseDifficulty()
    {
        // speed = Mathf.Min(maxSpeed, speed + 0.5f);
    }

    private void Activate()
    {
        dead = false;
        _spriteRenderer.color = Color.white;
        _rigidbody2D.simulated = true;
        _collider2D.enabled = true;
    }

    private void OnBecameInvisible()
    {
        if (dead)
        {
            Activate();
        }
    }
}