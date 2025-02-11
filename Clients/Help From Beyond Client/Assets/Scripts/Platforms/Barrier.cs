
using UnityEngine;

public class Barrier : MonoBehaviour
{
    enum BarrierType
    {
        Ghost,
        Key
    }

    [SerializeField] Color GhostBarrierColor;
    [SerializeField] Color KeyBarrierColor;

    [SerializeField] BarrierType _blocks = BarrierType.Ghost;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(11, 17);
        Physics2D.IgnoreLayerCollision(14, 16);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (_blocks == BarrierType.Ghost)
        {
            spriteRenderer.color = GhostBarrierColor;
            gameObject.layer = 16;
        }
        else
        {
            spriteRenderer.color = KeyBarrierColor;
            gameObject.layer = 17;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print(collision.gameObject.name);
    }
}
