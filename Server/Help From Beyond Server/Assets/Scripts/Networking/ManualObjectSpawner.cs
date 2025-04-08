using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ManualObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Prefab for the player
    [SerializeField] private GameObject pauseManagerPrefab; // Prefab for the pause manager
    [SerializeField] private GameObject roleTrackerPrefab; // Prefab for the role tracker
    [SerializeField] private GameObject enemyManagerPrefab; // Prefab for the enemy manager
    [SerializeField] private GameObject arcadeManagerPrefab; // Prefab for the arcade manager

    private PlayersStartingPoints _startingPoints; // Reference to the starting points
    private bool isListening = false; // Flag to check if the server is listening for connections

    // Track if the main player has already been spawned
    private bool hasSpawnedPauseManager = false;
    private bool hasSpawnedRoleTracker = false;
    private bool hasSpawnedArcadeManager = false;

    private List<ulong> connectedClients = new List<ulong>(); // List of connected clients

    private void Update()
    {
        // Wait for NetworkManager to be initialized and running as server
        if (!isListening && NetworkManager.Singleton != null
            && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening)
        {
            isListening = true;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            Debug.Log("ManualPlayerSpawner is now listening for client connections");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the client connected callback when the object is destroyed
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

        // Spawn pause manager if not already spawned
        if (!hasSpawnedPauseManager && pauseManagerPrefab != null)
        {
            GameObject pauseManagerInstance = Instantiate(pauseManagerPrefab);
            pauseManagerInstance.GetComponent<NetworkObject>().Spawn();
            hasSpawnedPauseManager = true;
            Debug.Log("Spawned NetworkPauseManager");
        }

        // Spawn role tracker if not already spawned
        if (!hasSpawnedRoleTracker && roleTrackerPrefab != null)
        {
            GameObject roleTrackerInstance = Instantiate(roleTrackerPrefab);
            roleTrackerInstance.GetComponent<NetworkObject>().Spawn();
            hasSpawnedRoleTracker = true;
            Debug.Log("Spawned RoleTracker");
        }

        // Spawn enemy manager if running as server
        if (NetworkManager.Singleton.IsServer && enemyManagerPrefab != null)
        {
            GameObject enemyManagerInstance = Instantiate(enemyManagerPrefab);
            enemyManagerInstance.GetComponent<NetworkObject>().Spawn();
            Debug.Log("Spawned EnemyManager on server");
        }

        // Spawn arcade manager if not already spawned
        if (!hasSpawnedArcadeManager && arcadeManagerPrefab != null)
        {
            GameObject arcadeManagerInstance = Instantiate(arcadeManagerPrefab);
            arcadeManagerInstance.GetComponent<NetworkObject>().Spawn();
            hasSpawnedArcadeManager = true;
            Debug.Log("Spawned ArcadeManager");
        }

        // Find starting points if not already found
        if (_startingPoints == null)
        {
            _startingPoints = FindObjectOfType<PlayersStartingPoints>();

            if (_startingPoints == null)
            {
                Debug.LogError("Starting points not found");
                return;
            }
        }

        // Spawn the player for the connected client
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

        Vector3 worldSpawn = points[index].position;
        worldSpawn.z = 0f; // Ensure z position is 0
        playerInstance.transform.position = worldSpawn;

        var rigidbody = playerInstance.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            rigidbody.velocity = Vector2.zero;
        }

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Role logic: index 0 = wizard, index 1 = ghost
        var playerManager = playerInstance.GetComponent<PlayerManager>();
        var role = index == 0 ? PlayerState.Wizard : PlayerState.Ghost;
        playerManager.SetInitialPlayerState(role);

        // Register the role on the server immediately
        var roleTracker = FindObjectOfType<PlayerRoleTracker>();
        if (roleTracker != null && roleTracker.IsServer)
        {
            if (role == PlayerState.Wizard)
            {
                roleTracker.RegisterWizard(clientId);
            }
            else
            {
                roleTracker.RegisterGhost(clientId);
            }
        }
        else
        {
            Debug.LogError("RoleTracker not found");
        }
    }
}