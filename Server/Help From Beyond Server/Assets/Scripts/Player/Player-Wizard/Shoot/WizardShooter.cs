using System;
using System.Collections;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;

public class WizardShooter : NetworkBehaviour
{
    public float distance, speed; // Distance and speed of the bullet
    private WizardValues _wizardValues; // Reference to WizardValues component
    [SerializeField] private Transform shooterSpriteTransform; // Transform for the shooter sprite
    private SpriteRenderer shooterSprite; // SpriteRenderer for the shooter sprite
    private MyInputManager _inputs; // Reference to input manager
    public bool ShootingEnabled = true; // Flag to enable/disable shooting
    public bool fastShootingEnabled = false; // Flag to enable/disable fast shooting
    private bool displayed = false; // Flag to check if the shooter is displayed
    [NonSerialized] public Bullet _bullet; // Reference to the bullet
    [SerializeField] private GameObject bulletTemplate; // Template for creating new bullets
    [SerializeField] private BulletPool _bulletPool; // Reference to the bullet pool

    void Start()
    {
        // Initialize input manager and shooter sprite
        _inputs = GetComponentInParent<MyInputManager>();
        shooterSprite = shooterSpriteTransform.GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Only the owner can control the shooter and shooting must be enabled
        if (!IsOwner || !ShootingEnabled) return;

        // Get the aim direction from the input manager
        Vector2 aim = _inputs.WizardAim();
        if (aim != Vector2.zero)
        {
            // Set the shooter position based on the aim direction
            SetSooterPosition(aim);
        }
        else
        {
            // Hide the shooter if there is no aim input
            if (displayed)
                HideShooter();
        }

        // Shoot the bullet if the shoot input is performed
        if (displayed && _inputs.WizardShootPerformedThisFrame())
        {
            SendShootRequestServerRpc(aim, fastShootingEnabled);
        }
    }

    private void SetSooterPosition(Vector2 direction)
    {
        // Set the position and rotation of the shooter sprite based on the aim direction
        Ray ray = new Ray(transform.position, direction);
        shooterSpriteTransform.position = ray.GetPoint(distance);
        shooterSpriteTransform.right = transform.position - shooterSpriteTransform.position;
        shooterSprite.enabled = true;

        displayed = true;
    }

    private void HideShooter()
    {
        // Hide the shooter sprite
        displayed = false;
        shooterSprite.enabled = false;
    }

    [ServerRpc]
    private void SendShootRequestServerRpc(Vector2 aimDirection, bool fastMode)
    {
        // Create a ray for the bullet's path
        Ray ray1 = new Ray(transform.position, aimDirection);
        Ray ray2 = new Ray(ray1.GetPoint(distance), ray1.direction);

        // Get a bullet from the bullet pool
        Bullet bullet = _bulletPool.GetBullet();

        // Set position and rotation before spawning
        bullet.transform.position = ray2.origin;
        bullet.transform.right = ray2.direction;

        // Spawn the bullet over the network
        bullet.NetworkObject.Spawn();

        // Server handles physics and VFX
        bullet.Shoot(ray2, speed);

        // Inform all clients to replicate the shoot VFX and position
        ulong bulletNetId = bullet.NetworkObject.NetworkObjectId;

        // Synchronize the bullet's position and rotation
        SyncBulletTransformClientRpc(ray2.origin, ray2.direction, bulletNetId);

        // Ask all clients to play sound and VFX
        PlayShootClientRpc(ray2.GetPoint(0.5f), bulletNetId);
    }

    [ClientRpc]
    private void SyncBulletTransformClientRpc(Vector3 position, Vector3 direction, ulong bulletNetworkId)
    {
        // Ignore if server (already running shooter logic)
        if (IsServer) return;

        StartCoroutine(ApplyTransformAfterSpawn(position, direction, bulletNetworkId));
    }

    private IEnumerator ApplyTransformAfterSpawn(Vector3 position, Vector3 direction, ulong bulletNetworkId)
    {
        // Wait one frame to ensure spawn is complete
        yield return null;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bulletNetworkId, out var bulletObj))
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet?.SetClientTransform(position, direction);
        }
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