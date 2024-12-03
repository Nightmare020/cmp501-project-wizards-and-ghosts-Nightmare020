
using UnityEngine;

public class Items : MonoBehaviour
{

    protected bool isPickedUp = false; // Comprobaci�n de si el objeto se ha cogido
    protected GameObject pickingPlayer; // Jugador que ha cogido el objeto
    protected bool hasBeenInteractedWith = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Physics2D.IgnoreLayerCollision(8, 3);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isPickedUp && Input.GetButton("Use Item"))
        {
            Debug.Log("Button Y Pressed for Use");
            Use();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que entra en el trigger es el jugador
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Player detected");
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        // Verificar si el objeto que entra en el trigger es el jugador
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")&&!IsPickedUp)
            // Comprobar si se ha pulsado el bot�n X
            // TODO: CAMBIAR INPUTS
            if (Input.GetButtonDown("Pick Up Item"))
            {
                Debug.Log("Button X Pressed for Pick Up");
                PickUp(collision.gameObject);
            }
    }

    public bool IsPickedUp
    {
        get { return isPickedUp; }
    }

    protected virtual void PickUp(GameObject player)
    {
        isPickedUp = true;
        pickingPlayer = player;
    }

    protected virtual void Use()
    {

    }
}
