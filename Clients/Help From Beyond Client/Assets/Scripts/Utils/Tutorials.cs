
using UnityEngine;

public class Tutorials : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerManager _playerManager;
    private WizardValues _wizardValues;
    [SerializeField] private CanvasGroup tutorial;
    [SerializeField] private float min = -2, max = -1;
    [SerializeField] private Transform tutorialPoint;


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(tutorialPoint.position, Mathf.Abs(min));
    }

    // Update is called once per frame
    void Update()
    {
        if (!_wizardValues || (_wizardValues && _wizardValues.transform.parent.CompareTag("ActiveGhost")))
        {
            GameObject aux = GameObject.FindGameObjectWithTag("ActiveWizard");
            if (aux)
            {
                _wizardValues = aux.GetComponentInChildren<WizardValues>();
            }
        }

        if (_wizardValues)
        {
            float distance = -Vector2.Distance(_wizardValues.transform.position, tutorialPoint.position);
            if (distance < min)
            {
                tutorial.alpha = 0;
            }
            else
            {
                tutorial.alpha = Mathf.Clamp01(MyUtils.Normalice(distance, min, max));
            }
        }
    }
}