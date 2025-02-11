using UnityEngine;

public class IgnoreCollisions : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Physics2D.IgnoreLayerCollision(6, 7); //bullet and player
        Physics2D.IgnoreLayerCollision(7, 7); //player and player
        Physics2D.IgnoreLayerCollision(7, 11); //player and Ghost
        Physics2D.IgnoreLayerCollision(3, 11); //ground and Ghost
        Physics2D.IgnoreLayerCollision(13,
            11); //walls and GhostPhysics2D.IgnoreLayerCollision(12, 11); //lava and Ghost
        Physics2D.IgnoreLayerCollision(10, 13); //ghost enemy and walls
        Physics2D.IgnoreLayerCollision(10, 3); //ghost enemy and ground
        Physics2D.IgnoreLayerCollision(10, 9); //ghost enemy and wizzard enemy
        Physics2D.IgnoreLayerCollision(10, 12); //ghost enemy and lava
        Physics2D.IgnoreLayerCollision(14, 9); //Bullet not enhanced and enemy
        Physics2D.IgnoreLayerCollision(11, 9); //ghsot and wizzard enemy
        Physics2D.IgnoreLayerCollision(6, 14); //ghsot and wizzard enemy
        Physics2D.IgnoreLayerCollision(10, 14); //ghsot and wizzard enemy



    }
}