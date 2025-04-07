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
        [SerializeField] private GameObject enemy;
        [SerializeField] private GameObject spawnsEnemies;

        private List<GhostEnemy> _enemies;
        private List<Transform> enemiesSpawnPoints;

        [SerializeField] private int maxEnemies = 2;
        [SerializeField] private float minSpawnDistance;

        private void Start()
        {
            if (!IsServer) return;

            _enemies = new List<GhostEnemy>();

            enemiesSpawnPoints = new List<Transform>();
            enemiesSpawnPoints.AddRange(spawnsEnemies.GetComponentsInChildren<Transform>());
            enemiesSpawnPoints.Remove(spawnsEnemies.transform);

            SpawnEnemies();
        }

        public void SpawnEnemies()
        {
            int enemiesSpawned = 0;
            int enemiesToSpawn = maxEnemies - _enemies.Count;

            while (enemiesSpawned < enemiesToSpawn)
            {
                Transform spawnPoint = enemiesSpawnPoints[Random.Range(0, enemiesSpawnPoints.Count)];
                Transform closestEnemy = GetClosestGhostEnemy(spawnPoint.position);
                if (closestEnemy == null || Vector2.Distance(spawnPoint.position,
                    closestEnemy.transform.position) > minSpawnDistance)
                {
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
            _enemies.Remove(enemy);
            StartCoroutine(RespawnEnemyAfterDelay(3f));
        }

        private IEnumerator RespawnEnemyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (ArcadeManager.Instance.IsGameOver)
                yield break;

            // Find a valid spawn point not near players
            Transform validPoint = GetValidSpawnPoint(5f); // min 5 units from players

            if (validPoint != null)
            {
                GameObject newGhost = Instantiate(enemy, validPoint.position, Quaternion.identity);
                var netObject = newGhost.GetComponent<NetworkObject>();
                netObject.Spawn();
                _enemies.Add(newGhost.GetComponent<GhostEnemy>());
            }
        }

        private Transform GetValidSpawnPoint(float minDistanceFromPlayers)
        {
            List<Transform> candidates = new List<Transform>();

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
            foreach (var player in allPlayers)
            {
                if (Vector3.Distance(player.transform.position, point) < minDist)
                    return false;
            }

            return true;
        }
    }
}