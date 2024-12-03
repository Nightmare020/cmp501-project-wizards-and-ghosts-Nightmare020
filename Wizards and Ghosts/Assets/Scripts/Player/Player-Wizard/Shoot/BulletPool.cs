using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    private Queue<Bullet> usedBullets;

    private Transform container;

    [SerializeField] private GameObject bulletTemplate;


    private void Awake()
    {
        usedBullets = new Queue<Bullet>();
    }

    private void Start()
    {
        transform.parent = null;
    }

    public Bullet GetBullet()
    {
        Bullet bullet = FindBullet();
        if (bullet != null)
            return bullet;
        GameObject newBullet = Instantiate(bulletTemplate, transform);
        Bullet bulletComp = newBullet.GetComponent<Bullet>();
        InsertNewBullet( bulletComp);
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

    private void InsertNewBullet(Bullet newBullet) =>
        usedBullets.Enqueue(newBullet);


}