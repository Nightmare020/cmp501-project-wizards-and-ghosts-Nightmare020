
using UnityEngine;

public class JumpSign : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private CanvasGroup JumpTutorial;
    [SerializeField] private float min = -2, max = -1;
    private WizardValues _wizardValues;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!_wizardValues)
        {
            PlayerManager otherPlayer = _playerManager.GetOtherPlayer();
            if (otherPlayer)
            {
                _wizardValues = otherPlayer.GetComponentInChildren<WizardValues>();
            }
        }

        if (_wizardValues && Time.frameCount % 3 == 0)
        {
            if (!_wizardValues.doubleJumpPerformed&& !_wizardValues.IsGrounded())
            {
                float distance = -Vector2.Distance(_wizardValues.transform.position, transform.position);
                if (distance < min)
                {
                    JumpTutorial.alpha = 0;
                }
                else
                {
                    JumpTutorial.alpha = Mathf.Clamp01(MyUtils.Normalice(distance, min, max));
                }
            }
            else
            {
                JumpTutorial.alpha = 0;
            }
        }
    }
}