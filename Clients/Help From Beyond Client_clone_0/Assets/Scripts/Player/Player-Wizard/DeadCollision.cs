
using UnityEngine;

public class DeadCollision : MonoBehaviour
{
    [SerializeField] private WizardValues _wizardValues;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.CompareTag($"Wizard Enemy"))
        {
            _wizardValues.Die();
        }
    }
}
