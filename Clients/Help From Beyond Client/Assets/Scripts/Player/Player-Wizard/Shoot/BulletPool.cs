using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    private Queue<Bullet> usedBullets; // Queue to store used bullets

    private Transform container; // Container to hold bullets

    [SerializeField] private GameObject bulletTemplate; // Template for creating new bullets

    private void Awake()
    {
        // Initialize the queue for used bullets
        usedBullets = new Queue<Bullet>();
    }

    private void Start()
    {
        // Disable the script if not running on the server
        if (!NetworkManager.Singleton.IsServer)
        {
            enabled = false;
            return;
        }

        // Detach the bullet pool from any parent
        transform.parent = null;
    }

    public Bullet GetBullet()
    {
        // Try to find an available bullet in the pool
        Bullet bullet = FindBullet();
        if (bullet != null)
            return bullet;

        // If no available bullet is found, create a new one
        GameObject newBullet = Instantiate(bulletTemplate, transform);
        Bullet bulletComp = newBullet.GetComponent<Bullet>();
        InsertNewBullet(bulletComp);
        return bulletComp;
    }

    private Bullet FindBullet()
    {
        int bulletCount = usedBullets.Count;
        for (int i = 0; i < bulletCount; i++)
        {
            Bullet bullet = usedBullets.Dequeue();
            usedBullets.Enqueue(bullet);
            if (!bullet.isBeingUsed)
            {
                bullet.isBeingUsed = true;
                return bullet;
            }
        }
        return null;
    }

    private void InsertNewBullet(Bullet newBullet)
    {
        // Add the new bullet to the queue
        usedBullets.Enqueue(newBullet);
    }
}