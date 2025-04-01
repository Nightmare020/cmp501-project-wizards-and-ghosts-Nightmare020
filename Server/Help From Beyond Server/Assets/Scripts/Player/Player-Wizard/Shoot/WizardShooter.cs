using System;

using UnityEngine;

public class WizardShooter : MonoBehaviour
{
    // Start is called before the first frame update
    public float distance, speed;
    private WizardValues _wizardValues;
    [SerializeField] private Transform shooterSpriteTransform;
    private SpriteRenderer shooterSprite;
    private MyInputManager _inputs;
    public bool ShootingEnabled = true;
    public bool fastShootiingEnabled = false;
    private bool displayed = false;
    [NonSerialized] public Bullet _bullet;
    [SerializeField] private GameObject bulletTemplate;
    [SerializeField] private BulletPool _bulletPool;


    void Start()
    {
        _inputs = GetComponentInParent<MyInputManager>();
        shooterSprite = shooterSpriteTransform.GetComponentInChildren<SpriteRenderer>();
        _bullet = Instantiate(bulletTemplate, null).GetComponent<Bullet>();
        _bullet.DisableBullet();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    private void Update()
    {
        if (ShootingEnabled)
        {
            Vector2 aim = _inputs.WizardAim();
            if (aim != Vector2.zero)
            {
                SetSooterPosition(aim);
            }
            else
            {
                if (displayed)
                    HideShooter();
            }

            if (displayed && _inputs.WizardShootPerformedThisFrame())
            {
                if (fastShootiingEnabled)
                {
                    //shoot
                    Ray ray1 = new Ray(transform.position, aim);
                    Ray ray2 = new Ray(ray1.GetPoint(distance), ray1.direction);
                    _bulletPool.GetBullet().Shoot(ray2, speed);
                }
                else
                {
                    if (!_bullet.isBeingUsed)
                    {
                        //shoot
                        Ray ray1 = new Ray(transform.position, aim);
                        Ray ray2 = new Ray(ray1.GetPoint(distance), ray1.direction);
                        _bullet.Shoot(ray2, speed);
                    }
                }
            }
        }
    }

    private void SetSooterPosition(Vector2 direction)
    {
        Ray ray = new Ray(transform.position, direction);
        shooterSpriteTransform.position = ray.GetPoint(distance);
        shooterSpriteTransform.right = transform.position - shooterSpriteTransform.position;
        shooterSprite.enabled = true;

        displayed = true;
    }

    private void HideShooter()
    {
        displayed = false;
        shooterSprite.enabled = false;
    }
}