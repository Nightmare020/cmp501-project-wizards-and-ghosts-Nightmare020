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
            _startingPoints = FindObjectOfType<PlayersStartingPoints>();
            if (_startingPoints == null)
            {
                Debug.LogError("Starting points not found in scene.");
            }

            Debug.Log("Manual player spawner is now listening for client connections");
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

        // Only spawns a player once (first client)
        if (!hasSpawnedMainPlayer)
        {
            hasSpawnedMainPlayer = true;
            SpawnPlayer(clientId);
        }
        else
        {
            Debug.Log($"Client {clientId} joined as spectator");
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        var playerInstance = Instantiate(playerPrefab);

        // Get the desired starting point
        Vector3 spawnPoint = Vector3.zero;

        if (_startingPoints != null)
        {
            List<Transform> startingPoints = _startingPoints.GetStartingPoints();
            //int index = NetworkManager.Singleton.ConnectedClients.Count - 1;

            if (startingPoints.Count > 0)
            {
                spawnPoint = startingPoints[0].position;
            }
        }
        // Set position before spawning the object
        playerInstance.transform.position = spawnPoint;

        // Spawn for the given client
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned player for client {clientId} at {spawnPoint}");
    }
}
