using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private EnemyAI enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemiesNumber = 10;
    [SerializeField] private Player player;

    private List<EnemyAI> spawnedEnemies = new List<EnemyAI>();
    private float timeSinceLastSpawn;
    private bool isSpawning = true;

    private void Update()
    {
        if (!isSpawning) return;
        
        CleanupDeadEnemies();
        
        timeSinceLastSpawn += Time.deltaTime;
        if(timeSinceLastSpawn >= spawnInterval)
        {
            TrySpawnEnemy();
            timeSinceLastSpawn = 0f;
        }
    }

    private void TrySpawnEnemy()
    {
        if (spawnedEnemies.Count < maxEnemiesNumber)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        int spawnIndex = spawnedEnemies.Count % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[spawnIndex];
        
        EnemyAI newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        newEnemy.Init(player, spawnPoint);
        spawnedEnemies.Add(newEnemy);
    }

    public void ResetSpawner()
    {
        // Clear existing enemies immediately
        foreach (EnemyAI enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        spawnedEnemies.Clear();
        
        // Reset spawn timer to force immediate spawn
        timeSinceLastSpawn = spawnInterval;
    }

    public void SetSpawningActive(bool active)
    {
        isSpawning = active;
        if (!active) CleanupDeadEnemies();
    }

    private void CleanupDeadEnemies()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
    }
}