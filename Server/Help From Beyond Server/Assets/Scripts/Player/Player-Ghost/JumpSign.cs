using UnityEngine;

public class JumpSign : MonoBehaviour
{
    // Reference to the PlayerManager component
    [SerializeField] private PlayerManager _playerManager;

    // Reference to the CanvasGroup for the jump tutorial
    [SerializeField] private CanvasGroup JumpTutorial;

    // Minimum and maximum distances for displaying the jump tutorial
    [SerializeField] private float min = -2, max = -1;

    // Reference to the WizardValues component
    private WizardValues _wizardValues;

    void Start()
    {
        // Initialization logic can be added here if needed
    }

    void Update()
    {
        // Check if the WizardValues reference is not set
        if (!_wizardValues)
        {
            // Get the other player (wizard) from the PlayerManager
            PlayerManager otherPlayer = _playerManager.GetOtherPlayer();
            if (otherPlayer)
            {
                // Get the WizardValues component from the other player
                _wizardValues = otherPlayer.GetComponentInChildren<WizardValues>();
            }
        }

        // Update the jump tutorial visibility every 3 frames
        if (_wizardValues && Time.frameCount % 3 == 0)
        {
            // Check if the wizard has not performed a double jump and is not grounded
            if (!_wizardValues.doubleJumpPerformed && !_wizardValues.IsGrounded())
            {
                // Calculate the distance between the wizard and the jump sign
                float distance = -Vector2.Distance(_wizardValues.transform.position, transform.position);
                // Hide the jump tutorial if the distance is less than the minimum
                if (distance < min)
                {
                    JumpTutorial.alpha = 0;
                }
                else
                {
                    // Show the jump tutorial with an alpha value based on the distance
                    JumpTutorial.alpha = Mathf.Clamp01(MyUtils.Normalice(distance, min, max));
                }
            }
            else
            {
                // Hide the jump tutorial if the wizard has performed a double jump or is grounded
                JumpTutorial.alpha = 0;
            }
        }
    }
}