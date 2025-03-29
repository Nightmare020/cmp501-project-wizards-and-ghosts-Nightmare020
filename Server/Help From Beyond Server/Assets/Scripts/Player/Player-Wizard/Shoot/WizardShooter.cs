using System;
using Unity.Netcode;
using UnityEngine;

public class WizardShooter : NetworkBehaviour
{
    // Start is called before the first frame update
    public float distance, speed;
    private WizardValues _wizardValues;
    [SerializeField] private Transform shooterSpriteTransform;
    private SpriteRenderer shooterSprite;
    private MyInputManager _inputs;
    public bool ShootingEnabled = true;
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
        if (!IsOwner || !ShootingEnabled) return;

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
                // Shoot
                Vector3 origin = shooterSpriteTransform.position;
                Vector2 dir = (origin - transform.position).normalized;

                ShootServerRpc(origin, dir);
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

    [ServerRpc]
    private void ShootServerRpc(Vector2 origin, Vector2 direction)
    {
        GameObject bulletObj = Instantiate(bulletTemplate, origin, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        // Set rotation and movement before spawning
        bullet.transform.right = direction;
        bullet.transform.position = origin;

        // Immediately enable and launch the bullet
        bullet.EnableBullet();
        bullet.Shoot(new Ray(origin, direction), speed);

        bulletObj.GetComponent<NetworkObject>().Spawn();
    }
}