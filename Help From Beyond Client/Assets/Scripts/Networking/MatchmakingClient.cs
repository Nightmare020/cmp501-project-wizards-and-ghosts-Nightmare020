using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;

public class MatchmakingClient : MonoBehaviour
{
    private HubConnection _hubConnection;
    public static MatchmakingClient Instance { get; private set; }

    public string MatchedRole { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    private async void Start()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5191/matchmaking") // replace with my server URL
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>("Matched", OnMatched);

        try
        {
            await _hubConnection.StartAsync();
            Debug.Log("Connected to server");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to connect to server: {ex.Message}");
        }
    }

    public async void SelectRole(string role)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SelectRole", role);
        }
        else
        {
            Debug.LogError("Not connected to server");
        }
    }

    private void OnMatched(string role)
    {
        MatchedRole = role;
        Debug.Log($"Matched as {role}");

        // Notify character selection class that a match was found
        SelectionWizardGhost.NotifyPlayerFound(true);
    }
}
