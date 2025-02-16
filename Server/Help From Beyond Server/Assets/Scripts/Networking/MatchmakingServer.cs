using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;
using Unity.Netcode;
using System.Runtime.CompilerServices;

public class MatchmakingServer : NetworkBehaviour
{
    private HubConnection _hubConnection;
    private SelectionWizardGhost _selectionPanel;
    public static MatchmakingServer Instance { get; private set; }
    public GameObject playerPrefab;
    private Dictionary<ulong, string> playerRoles = new Dictionary<ulong, string>();

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

        _selectionPanel = FindObjectOfType<SelectionWizardGhost>();
    }
    // Start is called before the first frame update
    private async void Start()
    {
        Debug.Log("Initializing hub connection...");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5077/matchmaking") // replace with my server URL
            .WithAutomaticReconnect()
            .Build();

        Debug.Log("Hub connection initialized");

        _hubConnection.On<string>("Matched", OnMatched);
        Debug.Log("Listening for 'Matched' events");

        try
        {
            await _hubConnection.StartAsync();
            Debug.Log("Connected to server");

            // Start Unity Netcode client if not started yet
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Starting Unity Netcode Client...");
                NetworkManager.Singleton.StartClient();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to connect to server: {ex.Message}");
        }
    }

    public async void SelectRole(string role)
    {
        if (_hubConnection == null)
        {
            Debug.LogError("Hub connection is not initialized yet");
        }

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
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SelectionWizardGhost.NotifyPlayerFound(true);
        });
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Debug.Log("Matchmaking Server started");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectRoleServerRpc(ulong clientId, string role)
    {
        if (!IsServer) return;

        // Assign role to player
        playerRoles[clientId] = role;
        Debug.Log($"Player {clientId} selected {role}");

        // If two players have joined, start the game
        if (playerRoles.Count == 2) 
        {
            StartMatch();
        }
    }

    private void StartMatch()
    {
        foreach (var role in playerRoles) 
        {
            ulong clientId = role.Key;
            string roleSelected = role.Value;

            // Spawn the player with assigned role
            GameObject newPlayer = Instantiate(playerPrefab);
            NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            // Assign role to the new player
            newPlayer.GetComponent<PlayerManager>().SetCurrentState(roleSelected == "Wizard" ? PlayerState.Wizard : PlayerState.Ghost);
        }
    }
}
