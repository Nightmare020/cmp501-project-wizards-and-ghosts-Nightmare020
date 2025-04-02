using System;
using Unity.Burst.Intrinsics;
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
    public bool fastShootingEnabled = false;
    private bool displayed = false;
    [NonSerialized] public Bullet _bullet;
    [SerializeField] private GameObject bulletTemplate;
    [SerializeField] private BulletPool _bulletPool;


    void Start()
    {
        _inputs = GetComponentInParent<MyInputManager>();
        shooterSprite = shooterSpriteTransform.GetComponentInChildren<SpriteRenderer>();
        //_bullet = Instantiate(bulletTemplate, null).GetComponent<Bullet>();
        //_bullet.DisableBullet();
    }

    private void Update()
    {
        if (!IsOwner || !ShootingEnabled) return;

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
            SendShootRequestServerRpc(aim, fastShootingEnabled);
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
    private void SendShootRequestServerRpc(Vector2 aimDirection, bool fastMode)
    {
        Ray ray1 = new Ray(transform.position, aimDirection);
        Ray ray2 = new Ray(ray1.GetPoint(distance), ray1.direction);

        Bullet bullet = _bulletPool.GetBullet();

        bullet.transform.position = ray2.origin;
        bullet.transform.right = ray2.direction;

        // Spawn the bullet over the network
        bullet.NetworkObject.Spawn();

        // Server handles physics and VFX
        bullet.Shoot(ray2, speed);

        // Inform all clients to replicate the shoot VFX and position
        ulong bulletNetId = bullet.NetworkObject.NetworkObjectId;

        // Ask all clients to play sound and VFX
        PlayShootClientRpc(ray2.GetPoint(0.5f), bulletNetId);
    }

    [ClientRpc]
    private void PlayShootClientRpc(Vector3 effectPosition, ulong bulletNetworkId)
    {
        // Ignore if server (already running shooter logic)
        if (IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bulletNetworkId, out var bulletObj))
        {
            var bullet = bulletObj.GetComponent<Bullet>();
            bullet?.PlayShootVFX(effectPosition);
        }
    }
}
