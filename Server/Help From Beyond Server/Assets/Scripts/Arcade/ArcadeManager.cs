using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ArcadeManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text liveScoreText; // Text component to display the live score
    [SerializeField] private TMP_Text timeText; // Text component to display the elapsed time

    private float timeCounter = 0f; // Counter to track time in seconds
    private bool gameOverTriggered = false; // Flag to check if the game over has been triggered

    private NetworkVariable<int> wizardScore = new NetworkVariable<int>(0); // Network variable to track the wizard's score

    public static ArcadeManager Instance { get; private set; } // Singleton instance of ArcadeManager

    private NetworkVariable<int> elapsedSeconds = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server); // Network variable to track elapsed seconds

    private void Awake()
    {
        Instance = this; // Set the singleton instance
    }

    private void Update()
    {
        // Only the server should update the time and check for game over
        if (!IsServer || gameOverTriggered) return;

        timeCounter += Time.deltaTime; // Increment the time counter by the time elapsed since the last frame

        // If one second has passed, update the elapsed seconds
        if (timeCounter >= 1f)
        {
            elapsedSeconds.Value++;
            timeCounter = 0f; // Reset the time counter
        }
    }

    private void OnEnable()
    {
        // Subscribe to the value changed events of the network variables
        wizardScore.OnValueChanged += UpdateScoreText;
        elapsedSeconds.OnValueChanged += UpdateTimeText;

        // If we're a client, manually initialize the score and time text
        if (!IsServer)
        {
            UpdateScoreText(0, wizardScore.Value);
            UpdateTimeText(0, elapsedSeconds.Value);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the value changed events of the network variables
        wizardScore.OnValueChanged -= UpdateScoreText;
        elapsedSeconds.OnValueChanged -= UpdateTimeText;
    }

    private void UpdateScoreText(int previous, int current)
    {
        // Update the live score text with the current score
        liveScoreText.text = current.ToString();
    }

    private void UpdateTimeText(int oldValue, int newValue)
    {
        // Calculate minutes and seconds from the elapsed seconds
        int minutes = newValue / 60;
        int seconds = newValue % 60;
        // Update the time text with the formatted time
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void AddWizardPoint()
    {
        // Only the server can add points to the wizard's score
        if (!IsServer) return;
        wizardScore.Value += 1;
    }

    public void TriggerGameOver()
    {
        // If the game over has already been triggered, do nothing
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        // Tell all clients to display their own pause menu
        ShowGameOverClientRpc(wizardScore.Value, elapsedSeconds.Value);
    }

    [ClientRpc]
    private void ShowGameOverClientRpc(int finalScore, int totalTime)
    {
        // Client-side logic to display the game over screen can be implemented here
    }

    public bool IsGameOver => gameOverTriggered; // Property to check if the game is over
}