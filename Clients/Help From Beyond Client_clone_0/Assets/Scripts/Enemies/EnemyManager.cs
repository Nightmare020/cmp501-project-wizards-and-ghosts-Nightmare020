using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemyManager : NetworkBehaviour
    {
        [SerializeField] private GameObject enemy; // Prefab of the enemy to spawn
        [SerializeField] private GameObject spawnsEnemies; // Parent object containing spawn points

        private List<GhostEnemy> _enemies; // List to keep track of spawned enemies
        private List<Transform> enemiesSpawnPoints; // List of spawn points

        [SerializeField] private int maxEnemies = 2; // Maximum number of enemies allowed
        [SerializeField] private float minSpawnDistance; // Minimum distance between spawn points and players

        private void Start()
        {
            // Only the server should manage enemy spawning
            if (!IsServer) return;

            _enemies = new List<GhostEnemy>();

            // Get all spawn points from the parent object
            enemiesSpawnPoints = new List<Transform>();
            enemiesSpawnPoints.AddRange(spawnsEnemies.GetComponentsInChildren<Transform>());
            enemiesSpawnPoints.Remove(spawnsEnemies.transform);

            // Spawn initial enemies
            SpawnEnemies();
        }

        public void SpawnEnemies()
        {
            int enemiesSpawned = 0;
            int enemiesToSpawn = maxEnemies - _enemies.Count;

            // Spawn enemies until the maximum number is reached
            while (enemiesSpawned < enemiesToSpawn)
            {
                Transform spawnPoint = enemiesSpawnPoints[Random.Range(0, enemiesSpawnPoints.Count)];
                Transform closestEnemy = GetClosestGhostEnemy(spawnPoint.position);

                // Check if the spawn point is far enough from other enemies
                if (closestEnemy == null || Vector2.Distance(spawnPoint.position,
                    closestEnemy.transform.position) > minSpawnDistance)
                {
                    // Instantiate and spawn the enemy
                    GameObject newGhost = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
                    var netObject = newGhost.GetComponent<NetworkObject>();
                    netObject.Spawn();
                    _enemies.Add(newGhost.GetComponent<GhostEnemy>());
                    enemiesSpawned++;
                }
            }
        }

        public Transform GetClosestGhostEnemy(Vector2 position)
        {
            float min = Single.PositiveInfinity;
            Transform result = null;

            // Find the closest enemy to the given position
            for (int i = 0; i < _enemies.Count; i++)
            {
                float dist = Vector2.Distance(position, _enemies[i].transform.position);
                if (dist < min)
                {
                    min = dist;
                    result = _enemies[i].transform;
                }
            }

            return result;
        }

        public void OnEnemyDied(GhostEnemy enemy)
        {
            // Remove the dead enemy from the list and start the respawn coroutine
            _enemies.Remove(enemy);
            StartCoroutine(RespawnEnemyAfterDelay(3f));
        }

        private IEnumerator RespawnEnemyAfterDelay(float delay)
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(delay);

            // If the game is over, do not respawn enemies
            if (ArcadeManager.Instance.IsGameOver)
                yield break;

            // Find a valid spawn point not near players
            Transform validPoint = GetValidSpawnPoint(5f); // min 5 units from players

            if (validPoint != null)
            {
                // Instantiate and spawn the enemy
                GameObject newGhost = Instantiate(enemy, validPoint.position, Quaternion.identity);
                var netObject = newGhost.GetComponent<NetworkObject>();
                netObject.Spawn();
                _enemies.Add(newGhost.GetComponent<GhostEnemy>());
            }
        }

        private Transform GetValidSpawnPoint(float minDistanceFromPlayers)
        {
            List<Transform> candidates = new List<Transform>();

            // Find spawn points that are far enough from all players
            foreach (var point in enemiesSpawnPoints)
            {
                if (IsFarFromAllPlayers(point.position, minDistanceFromPlayers))
                {
                    candidates.Add(point);
                }
            }

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        private bool IsFarFromAllPlayers(Vector3 point, float minDist)
        {
            var allPlayers = GameObject.FindGameObjectsWithTag("Player");
            // Check if the point is far enough from all players
            foreach (var player in allPlayers)
            {
                if (Vector3.Distance(player.transform.position, point) < minDist)
                    return false;
            }

            return true;
        }
    }
}