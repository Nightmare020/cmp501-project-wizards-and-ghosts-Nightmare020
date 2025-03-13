using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public bool hasBeenInteractedWith = false;

    // Propiedades para el efecto de flotaci�n
    [SerializeField] float floatHeight = 0.2f;
    [SerializeField] float floatSpeed = 0.3f;

    [SerializeField] Sprite[] sprites;
    public bool _inWizardReality = true;

    private SpriteRenderer spriteRenderer;
    private float initialYPosition;
    private Vector2 initialPosition;
    private Rigidbody2D rb;

    protected  void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        initialYPosition = transform.position.y;
        initialPosition = transform.position;
    }

    protected void Update()
    {
        if (!hasBeenInteractedWith)
        {
            // Efecto de flotacion
            FloatEffect();
        }
    }

    private void FloatEffect()
    {
        float newY = initialYPosition + Mathf.PingPong(Time.time * floatSpeed, floatHeight * 1) - floatHeight;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }

    public void Interact()
    {
        hasBeenInteractedWith = true;
        rb.isKinematic = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trampoline"))
        {
            Bounce();
        }

        if (collision.CompareTag("Lava"))
        {
            hasBeenInteractedWith = false;
            transform.eulerAngles = new Vector3(0, 0, 50);
            rb.velocity = Vector2.zero;
            _inWizardReality = true;
            // Toggle sprite
            spriteRenderer.sprite = sprites[0];
            transform.position = initialPosition;
        }
    }

    public void Bounce()
    {
        // Toggle reality logic
        _inWizardReality = !_inWizardReality;

        // Toggle sprite
        spriteRenderer.sprite = _inWizardReality ? sprites[0] : sprites[1];
    }

}
