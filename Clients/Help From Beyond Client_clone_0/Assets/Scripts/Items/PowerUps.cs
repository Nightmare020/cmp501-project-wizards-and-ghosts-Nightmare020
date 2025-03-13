
using UnityEngine;

public class PowerUps : Items
{

    // Escala inicial del objeto en el eje Y
    private Vector3 originalScale;

    [SerializeField] float hearbeatSpeed = 15.0f;

    // Amplitud m�xima y m�nima para evitar escalas excesivas
    [SerializeField] float maxAmplitude = 0.05f;
    [SerializeField] float minAmplitude = -0.05f;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Start de la clase padre
        base.Start();

        // Guardamos la escala inicial del objeto en el eje Y
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Metodo para que objeto lata como un coraz�n
        Latent();

        // Update de la clase padre
        base.Update();
    }

    private void Latent()
    {
        // Usa la funci�n sinusoidal para simular el latido del coraz�n
        float scaleChange = Mathf.Sin(Time.time * hearbeatSpeed) * 0.1f; // Factor de escala predeterminado

        // Limita la escala dentro del rango especificado
        scaleChange = Mathf.Clamp(scaleChange, minAmplitude, maxAmplitude);

        // Aplica la escala en ambos ejes
        transform.localScale = originalScale + new Vector3(scaleChange, scaleChange, 0f);

    }

    protected override void PickUp(GameObject player)
    {
        base.PickUp(player);

        Debug.Log("Ha entrado en el Pickup de power ups");

        if (player.CompareTag("Wizard") && gameObject.CompareTag("Wizard"))
        {
            Debug.Log("Jugador mago va a coger power up para el");
            gameObject.SetActive(false); // Desactiva el objeto al recogerlo
        }
    }
}
