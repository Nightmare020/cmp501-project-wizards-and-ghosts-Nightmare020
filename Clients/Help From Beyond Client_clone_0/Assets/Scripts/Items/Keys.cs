using System.Collections;
using UnityEngine;

public class Keys : Items
{

    // Propiedades para el efecto de flotaci�n
    [SerializeField] float floatHeight = 0.3f;
    [SerializeField] float floatSpeed = 0.4f;

    // Propiedades para el efecto de brillo
    [SerializeField] Material shiningMaterial;
    private Material originalMaterial;
    [SerializeField] float emissionIntensity = 1.0f; // Ajusta la intensidad del brillo

    // Posici�n inicial del objeto en el eje Y
    private float initialYPosition;

    // Controladores de animaci�n
    [SerializeField] Animator doorAnimator;
    [SerializeField] Animator lockAnimator;
    [SerializeField] Animator keyAnimator;

    private bool isKeyUsed = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Start de la clase padre
        base.Start();

        // Guardar el material original para restaurarlo despu�s del brillo
        originalMaterial = GetComponent<Renderer>().material;

        // Guardamos la posici�n inicial en la que se encuentra el objeto en el eje Y
        initialYPosition = transform.localPosition.y;

        if (doorAnimator != null)
        {
            doorAnimator.enabled = false;
        }

        if (lockAnimator != null)
        {
            lockAnimator.enabled = false;
        }

        if (keyAnimator != null)
        {
            keyAnimator.enabled = false;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Update de la clase padre
        base.Update();

        // Efecto de flotaci�n
        FloatEffect();
        
        // Efecto de brillo
        ShineEffect();
    }

    private void FloatEffect()
    {
        float newY = initialYPosition + Mathf.PingPong(Time.time * floatSpeed, floatHeight * 1) - floatHeight;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }

    private void ShineEffect()
    {
        if (shiningMaterial != null)
        {
            float lerp = Mathf.PingPong(Time.time, 1.0f);
            Material blendedMaterial = GetComponent<Renderer>().material;

            // Ajustar la intensidad del brillo
            blendedMaterial.SetColor("_EmissionColor", shiningMaterial.GetColor("_EmissionColor") * emissionIntensity);

            // Aplicar la mezcla de materiales
            GetComponent<Renderer>().material.Lerp(originalMaterial, blendedMaterial, lerp);
        }
    }

    protected override void PickUp(GameObject player)
    {

        Debug.Log("Ha entrado en el Pickup de llaves");

        if (player.CompareTag("Wizard") && gameObject.CompareTag("Wizard"))
        {
            Debug.Log("Jugador " + player.tag + " va a coger llave de tipo " + gameObject.tag);
            base.PickUp(player);
        }
        else if (player.CompareTag("Ghost") && gameObject.CompareTag("Ghost"))
        {
            Debug.Log("Jugador " + player.tag + " va a coger llave de tipo " + gameObject.tag);
            base.PickUp(player);
        }

        else
        {
            // La llave no es del tipo adecuado para el jugador actual
            Debug.Log("Wrong type of key for the current player " + player.tag);
            //isPickedUp = false;
            //pickingPlayer = null;
        }

    }

    protected override void Use()
    {
        // Verifica si el jugador tiene la llave y est� cerca de la puerta
        if (IsPickedUp && pickingPlayer != null && pickingPlayer.GetComponent<Collider2D>().IsTouching(GetComponent<Collider2D>()))
        {
            Debug.Log("Jugador est� en contacto con la puerta");

            StartCoroutine(DoorAnimationSequence());
        }
    }

    private IEnumerator DoorAnimationSequence()
    {
        // 1. Mover la llave al punto inicial
        MoveKeyToStartingPoint();

        // Esperar a que termine la animaci�n de la llave
        yield return new WaitForSeconds(GetAnimatorLength(keyAnimator) - 10f);

        // 2. Iniciar la animaci�n del candado
        lockAnimator.enabled = true;
        lockAnimator.Play("Unlock");

        // Esperar a que la animaci�n del candado termine
        yield return new WaitForSeconds(GetAnimatorLength(lockAnimator));

        // 3. Iniciar la animaci�n de la puerta
        doorAnimator.enabled = true;
        doorAnimator.Play("DoorOpen");

        // Esperar a que la animaci�n de la puerta termine
        yield return new WaitForSeconds(GetAnimatorLength(doorAnimator));

        // Destruir la llave y el candado
        Destroy(gameObject);
        Destroy(lockAnimator.gameObject);
    }

    private void MoveKeyToStartingPoint()
    {
        Debug.Log("Se va a mover la llave a la posici�n de animaci�n");
        Transform keyStartingPoint = gameObject.transform.Find("KeyAnimStartPosition");

        if (keyStartingPoint != null && !isKeyUsed)
        {
            Debug.Log("Se ha encontrado la posici�n de animaci�n de la llave");

            // Coloca la llave en la posici�n inicial
            transform.position = keyStartingPoint.position;

            Debug.Log("Llave colocada en posici�n");

            keyAnimator.enabled = true;
            keyAnimator.Play("KeyAnimation");

            isKeyUsed = true;
        }
    }

    private float GetAnimatorLength(Animator animator)
    {
        // Obtiene la duraci�n total de la animaci�n
        float length = 0f;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                length += clip.length;
            }
        }

        return length;
    }
}
