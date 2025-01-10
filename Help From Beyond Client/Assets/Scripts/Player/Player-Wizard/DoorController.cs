using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] GameObject keyStartingPoint; // Asigna el objeto "KeyStartingPoint" desde el inspector

    // COntroladores de animaci�n
    [SerializeField] Animator doorAnimator;
    [SerializeField] Animator lockAnimator;
    [SerializeField] Animator keyAnimator;

    private bool isKeyUsed = false;

    // Start is called before the first frame update
    void Start()
    {
        if (doorAnimator == null || lockAnimator == null || keyAnimator == null)
        {
            Debug.LogError("Falta asignar las animaciones en el inspector");
        }

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
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

    }

    public void StartDoorAnimation()
    {
        StartCoroutine(DoorAnimationSequence());
    }

    private IEnumerator DoorAnimationSequence()
    {
        // 1. Mover la llave al punto inicial
        MoveKeyToStartingPoint();

        // Esperar a que termine la animaci�n de la llave
        yield return new WaitForSeconds(GetAnimatorLength(keyAnimator));

        // 2. Iniciar la animaci�n del candado
        lockAnimator.enabled = true;
        lockAnimator.Play("Unlock");

        // Esperar a que la animaci�n del candado termine
        yield return new WaitForSeconds(GetAnimatorLength(lockAnimator));

        // 3. Iniciar la animaci�n de la puerta
        doorAnimator.enabled = true;
        doorAnimator.Play("DoorOpen");
    }

    private void MoveKeyToStartingPoint()
    {
        if (keyStartingPoint != null && !isKeyUsed)
        {
            // Coloca la llave en la posici�n inicial
            keyStartingPoint.transform.position = keyStartingPoint.transform.position;

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
