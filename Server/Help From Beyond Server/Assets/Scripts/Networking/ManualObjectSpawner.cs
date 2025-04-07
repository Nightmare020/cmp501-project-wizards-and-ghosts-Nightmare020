using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ManualObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject pauseManagerPrefab;
    [SerializeField] private GameObject roleTrackerPrefab;
    [SerializeField] private GameObject enemyManagerPrefab;
    [SerializeField] private GameObject arcadeManagerPrefab;

    private PlayersStartingPoints _startingPoints;
    private bool isListening = false;

    // Track if the main player has already been spawned
    private bool hasSpawnedPauseManager = false;
    private bool hasSpawnedRoleTracker = false;
    private bool hasSpawnedArcadeManager = false;

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

        // Spawn pause manager
        if (!hasSpawnedPauseManager && pauseManagerPrefab != null)
        {
            GameObject pauseManagerInstance = Instantiate(pauseManagerPrefab);
            pauseManagerInstance.GetComponent<NetworkObject>().Spawn();
            hasSpawnedPauseManager = true;
            Debug.Log("Spawned NetworkPauseManager");
        }

        if (!hasSpawnedRoleTracker && roleTrackerPrefab != null)
        {
            GameObject roleTrackerInstance = Instantiate(roleTrackerPrefab);
            roleTrackerInstance.GetComponent<NetworkObject>().Spawn();
            hasSpawnedRoleTracker = true;
            Debug.Log("Spawned RoleTracker");
        }

        if (NetworkManager.Singleton.IsServer && enemyManagerPrefab != null)
        {
            GameObject enemyManagerInstance = Instantiate(enemyManagerPrefab);
            enemyManagerInstance.GetComponent<NetworkObject>().Spawn();
            Debug.Log("Spawned EnemyManager on server");
        }

        if (!hasSpawnedArcadeManager && arcadeManagerPrefab != null)
        {
            GameObject arcadeManagerInstance = Instantiate(arcadeManagerPrefab);
            arcadeManagerInstance.GetComponent<NetworkObject>().Spawn();
            hasSpawnedArcadeManager = true;
            Debug.Log("Spawned ArcadeManager");
        }

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

        // register the role on the server immediately
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
