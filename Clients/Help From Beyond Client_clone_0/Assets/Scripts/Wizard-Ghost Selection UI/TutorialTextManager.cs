
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialTextManager : MonoBehaviour
{
    [SerializeField] string tutorialText;
    [SerializeField] TextMeshProUGUI displayText;
    [SerializeField] Image displayImage;
    [SerializeField] Sprite button;

    private void Start()
    {
        if (displayText != null)
        {
            displayText.text = "";
        }

        if (displayImage != null)
        {
            displayImage.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wizard"))
        {
            if (displayText != null)
            {
                displayText.text = tutorialText;
            }

            if (displayImage != null)
            {
                displayImage.sprite = button;
                displayImage.enabled = true;
            }
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Wizard"))
        {
            if (displayText != null)
            {
                displayText.text = "";
            }

            if (displayImage != null)
            {
                displayImage.enabled = false;
            }
        }
        
    }
}
