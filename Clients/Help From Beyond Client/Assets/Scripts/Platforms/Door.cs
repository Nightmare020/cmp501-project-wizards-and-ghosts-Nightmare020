using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Transform keyAnimationStart;
    private GameObject key;
    private float lerpValue = 0;


    private bool keyInPosition = false;
    private Animator keyAnimator, lockAnimator, doorAnimator;

    [SerializeField] Transform lockObj;
    [SerializeField] GameObject lockGO;
    [SerializeField] GameObject levelFinish;

    private void Start()
    {
        lockAnimator = GetComponentInChildren<Animator>();
        doorAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("InstanceItem"))
        {
            key = other.gameObject;
            keyAnimator = key.GetComponent<Animator>();
            //if (key.transform.parent.tag == "Wizard")
            //{
            //    key.transform.parent.Find("" GetComponent<ItemPickUp>().ReleaseKey();
            //}
            //else if (key.transform.parent.tag == "Ghost")
            //{
                key.transform.parent.GetComponent<ItemPickUp>().ReleaseKey();
            //}
            
            key.GetComponent<Collider2D>().enabled = false;
            
        }
    }

    private void Update()
    {
        if (key)
        {
            if (!keyInPosition) MoveKeyNearDoor();
            else MoveKeyToDoor();
        }
    }

    private void MoveKeyNearDoor()
    {
        Vector3 keyInitialPosition = key.transform.position;
        key.transform.position = Vector3.Lerp(keyInitialPosition, keyAnimationStart.position, lerpValue);

        if (lerpValue < 1) lerpValue += 1 * Time.deltaTime;
        else
        {
            lerpValue = 0;
            keyInPosition = true;
            print("done1");
        }
    }

    private void MoveKeyToDoor()
    {
        print("done2");
        Vector3 keyInitialPosition = key.transform.position;
        key.transform.position = Vector3.Lerp(keyInitialPosition, lockObj.position, lerpValue);

        if (lerpValue < 1) lerpValue += 1 * Time.deltaTime;
        else StartCoroutine(DoorAnimationSequence(key));
    }

    private IEnumerator DoorAnimationSequence(GameObject key)
    {
        keyAnimator.enabled = true;
        keyAnimator.SetTrigger("Use");

        // Esperar a que termine la animaci�n de la llave
        yield return new WaitForSeconds(GetAnimatorLength(keyAnimator));

        // 2. Iniciar la animaci�n del candado
        lockAnimator.SetTrigger("Unlock");

        // Esperar a que la animaci�n del candado termine
        yield return new WaitForSeconds(GetAnimatorLength(lockAnimator));

        // 3. Iniciar la animaci�n de la puerta
        doorAnimator.SetTrigger("Unlock");

        EnableLevelFinish();

        yield return new WaitForSeconds(1);

        Destroy(key);
        lockGO.SetActive(false);
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

    private void EnableLevelFinish()
    {
        levelFinish.SetActive(true);
    }

}
