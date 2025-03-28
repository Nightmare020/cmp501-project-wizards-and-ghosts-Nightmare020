using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ManualPlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        // Wait until NetworkManager is ready
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in scene.");
            return;
        }

        // Subscribe to connection callback
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // If we're the server, decide who gets a player
        if (!NetworkManager.Singleton.IsServer) return;

        // Only spawn a wizard for the first player
        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            SpawnPlayer(clientId);
            Debug.Log($"Spawned player wizard for client {clientId}");
        }
        else
        {
            Debug.Log($"Client {clientId} has other player emulating.");
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        var playerInstance = Instantiate(playerPrefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
