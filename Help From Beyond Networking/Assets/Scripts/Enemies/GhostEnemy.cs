
using UnityEngine;

public class GhostEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;

    Camera camera;
    private bool dead = false;

    [SerializeField] private Vector2 direction = new Vector2(1, 0);
    [SerializeField] private float speed = 1, normalSpeed = 1, maxSpeed = 7;
    [SerializeField] private float minDistToWizard = 10, minMinDistance = 1;
    [SerializeField] private Color angerColor;

    //wizard
    private WizardValues _wizardValues;
    private GhostValues _ghostValues;

    void Start()
    {
        camera = Camera.main;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D.gravityScale = 0;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistToWizard);
    }

    private void FixedUpdate()
    {
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
                if (Time.frameCount % 2 == 0)
                {
                    //position relative to the wizard
                    float relPos = (transform.position - _wizardValues.transform.position).x;
                    if (!_wizardValues._playerManager.isDead && (_wizardValues.facingDirection * relPos) > 0 &&
                        Vector2.Distance(
                            _wizardValues.transform.position, transform.position) < minDistToWizard)
                    {
                        speed = 0;
                    }
                    else
                    {
                        speed = normalSpeed;
                    }
                }

                if (_rigidbody2D.velocity.x < 0)
                {
                    _spriteRenderer.flipX = true;
                }
                else
                {
                    _spriteRenderer.flipX = false;
                }


                Vector2 direction = Vector2.zero;
                //chase Ghost
                if (!_ghostValues._playerManager.isDead)
                {
                    _spriteRenderer.color = Color.white;
                    direction = (_ghostValues.transform.position - transform.position).normalized;
                }
                //Chase wizard if ghost is dead
                else
                {
                    _spriteRenderer.color = angerColor;
                    direction = (_wizardValues.transform.position - transform.position).normalized;
                }

                _rigidbody2D.AddForce(direction * speed - _rigidbody2D.velocity);
            }
        }
    }

    public void IncreaseDifficulty()
    {
        normalSpeed = Mathf.Min(maxSpeed, speed + 1f);
        minDistToWizard = Mathf.Max(minMinDistance, minDistToWizard - 1f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
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
            //kill player
            GetWizard()._playerManager.Die();
            Die();
        }
    }

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

    public void Die()
    {
        dead = true;
        _spriteRenderer.color = Color.clear;
        _rigidbody2D.simulated = false;
        _collider2D.enabled = false;
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