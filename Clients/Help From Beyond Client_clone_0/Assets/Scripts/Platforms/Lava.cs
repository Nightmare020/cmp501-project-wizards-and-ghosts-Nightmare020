using System.Collections;
using UnityEngine;

public class Lava : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform checkPoint;

    [SerializeField] float deathTimer = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wizard"))
        {
            other.GetComponentInParent<ItemPickUp>().DropKey();
            WizardValues wizardValues = other.GetComponentInParent<WizardValues>();
            StartCoroutine(GetInLavaCoroutine(wizardValues));
        }
    }

    IEnumerator GetInLavaCoroutine(WizardValues wizardValues)
    {
        wizardValues.rigidBody.drag = 10;

        yield return new WaitForSeconds(deathTimer);
        wizardValues.Die(checkPoint.position);
        wizardValues.rigidBody.drag = wizardValues.drag;
    }
}