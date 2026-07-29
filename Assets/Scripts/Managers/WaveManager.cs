using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private WaveData currentWave; // Drag Wave1Data here
    [SerializeField] private float spawnXPosition = 9f; // Just off the right edge of the screen
    [SerializeField] private float minY = -4f;         // Bottom boundary of the screen
    [SerializeField] private float maxY = 4f;          // Top boundary of the screen

    private bool isSpawning = false;

    private void Start()
    {
        if (currentWave != null)
        {
            StartCoroutine(SpawnWaveRoutine());
        }
        else
        {
            Debug.LogWarning("[CoreDefender] No WaveData assigned to WaveManager!");
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;
        Debug.Log($"[CoreDefender] Starting Wave: {currentWave.waveName} (Total Enemies: {currentWave.enemyCount})");

        // Wait a brief moment before starting wave spawns
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"[CoreDefender] Wave {currentWave.waveName} spawn sequence completed.");
    }

    private void SpawnEnemy()
    {
        if (currentWave.enemyPrefab != null)
        {
            // Pick a random Y height between minY and maxY on the right side of the screen
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(spawnXPosition, randomY, 0f);

            Instantiate(currentWave.enemyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"[CoreDefender] Spawned wave enemy unit at Y: {randomY:F2}");
        }
    }
}