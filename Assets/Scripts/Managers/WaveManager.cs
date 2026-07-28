using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private WaveData currentWave; // Drag Wave1Data here
    [SerializeField] private Vector3 spawnPosition = new Vector3(15f, 4.5f, 0f);

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
            Instantiate(currentWave.enemyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("[CoreDefender] Spawned wave enemy unit.");
        }
    }
}