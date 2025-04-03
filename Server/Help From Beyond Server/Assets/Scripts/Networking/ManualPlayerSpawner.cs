using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ManualPlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private PlayersStartingPoints _startingPoints;
    private bool isListening = false;

    // Track if the main player has already been spawned
    private bool hasSpawnedMainPlayer = false;

    private List<ulong> connectedClients = new List<ulong>();

    private void Update()
    {
        // Wait for NetworkManager to be initialized and running as server
        if (!isListening && NetworkManager.Singleton != null
            && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening)
        {
            isListening = true;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            Debug.Log("ManualPlayerSpawner is now listening for client connections");

            // Get reference to spawn point manager
            //_startingPoints = FindObjectOfType<PlayersStartingPoints>();
            //if (_startingPoints == null)
            //{
            //    Debug.LogError("Starting points not found in scene.");
            //}

            //Debug.Log("Manual player spawner is now listening for client connections");
        }
    }

    private void OnDestroy()
    {
        if (isListening && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"On client connected: {clientId}");

        connectedClients.Add(clientId);
        int index = connectedClients.Count - 1;

        if (_startingPoints == null)
        {
            _startingPoints = FindObjectOfType<PlayersStartingPoints>();

            if (_startingPoints == null)
            {
                Debug.LogError("Starting points not found");
                return;
            }
        }

        SpawnPlayer(clientId, index);
    }

    private void SpawnPlayer(ulong clientId, int index)
    {
        GameObject playerInstance = Instantiate(playerPrefab);

        // Set correct spawn point
        var points = _startingPoints.GetStartingPoints();

        if (points.Count <= index)
        {
            Debug.LogError("Not enough starting points for all players");
            return;
        }

        playerInstance.transform.position = points[index].localPosition;

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Role logic: index 0 = wizard, index 1 = ghost
        var playerManager = playerInstance.GetComponent<PlayerManager>();
        var role = index == 0 ? PlayerState.Wizard : PlayerState.Ghost;
        playerManager.SetInitialPlayerState(role);
    }
}
