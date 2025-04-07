using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ArcadeManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text liveScoreText;
    [SerializeField] private TMP_Text timeText;

    private float timeCounter = 0f;
    private bool gameOverTriggered = false;

    private NetworkVariable<int> wizardScore = new NetworkVariable<int>(0);

    public static ArcadeManager Instance { get; private set; }

    private NetworkVariable<int> elapsedSeconds = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer || gameOverTriggered) return;

        timeCounter += Time.deltaTime;

        if (timeCounter >= 1f)
        {
            elapsedSeconds.Value++;
            timeCounter = 0f;
        }
    }

    private void OnEnable()
    {
        wizardScore.OnValueChanged += UpdateScoreText;
        elapsedSeconds.OnValueChanged += UpdateTimeText;

        // If we're a client, manually intialize the score text
        if (!IsServer)
        {
            UpdateScoreText(0, wizardScore.Value);
            UpdateTimeText(0, elapsedSeconds.Value);
        }
    }

    private void OnDisable()
    {
        wizardScore.OnValueChanged -= UpdateScoreText;
        elapsedSeconds.OnValueChanged -= UpdateTimeText;
    }

    private void UpdateScoreText(int previous, int current)
    {
        liveScoreText.text = current.ToString();
    }

    private void UpdateTimeText(int oldValue, int newValue)
    {
        int minutes = newValue / 60;
        int seconds = newValue % 60;
        timeText.text = $"{minutes:00}:{seconds:00}";
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

        // Tell all clients to dipaly their own pause menu
        ShowGameOverClientRpc(wizardScore.Value, elapsedSeconds.Value);
    }

    [ClientRpc]
    private void ShowGameOverClientRpc(int finalScore, int totalTime)
    {

    }

    public bool IsGameOver => gameOverTriggered;
}