using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ArcadeManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text liveScoreText;

    private float elapsedTime = 0f;
    private bool gameOverTriggered = false;

    private NetworkVariable<int> wizardScore = new NetworkVariable<int>(0);

    public static ArcadeManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer || gameOverTriggered) return;

        elapsedTime += Time.deltaTime;
    }

    private void OnEnable()
    {
        wizardScore.OnValueChanged += UpdateScoreText;

        // If we're a client, manually intialize the score text
        if (!IsServer)
        {
            UpdateScoreText(0, wizardScore.Value);
        }
    }

    private void OnDisable()
    {
        wizardScore.OnValueChanged -= UpdateScoreText;
    }

    private void UpdateScoreText(int previous, int current)
    {
        liveScoreText.text = current.ToString();
    }

    public void AddWizardPoint()
    {
        if (!IsServer) return;
        wizardScore.Value += 1;
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;
    }

    public bool IsGameOver => gameOverTriggered;
}