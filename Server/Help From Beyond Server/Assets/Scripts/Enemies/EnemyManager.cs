using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemy, spawnsEnemies;

        private List<GhostEnemy> _enemies;

        [SerializeField] private int maxEnemies = 2;
        private List<Transform> enemiesSpawnPoints;

        [SerializeField] private float minSpawnDistance;

        private void Start()
        {
            _enemies = new List<GhostEnemy>();

            enemiesSpawnPoints = new List<Transform>();
            enemiesSpawnPoints.AddRange(spawnsEnemies.GetComponentsInChildren<Transform>());
            enemiesSpawnPoints.Remove(spawnsEnemies.transform);


            SpawnEnemies();
        }

        public void SpawnEnemies()
        {
            //ghostenemies
            int enemiesSpawned = 0;
            int enemiesToSpawn = maxEnemies - _enemies.Count;

            while (enemiesSpawned < enemiesToSpawn)
            {
                Transform spawnPoint = enemiesSpawnPoints[Random.Range(0, enemiesSpawnPoints.Count)];
                Transform closestEnemy = GetClosestGhostEnemy(spawnPoint.position);
                if (closestEnemy == null || (closestEnemy != null &&
                                                  Vector2.Distance(spawnPoint.position,
                                                      closestEnemy.transform.position) > minSpawnDistance))
                {
                    GameObject newGhost = Instantiate(enemy, transform);
                    newGhost.transform.position = spawnPoint.position;
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
    }
}