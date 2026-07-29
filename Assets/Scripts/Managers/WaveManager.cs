using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI waveAnnouncementText; 

    [Header("Wave Base Settings")]
    [SerializeField] private GameObject standardEnemyPrefab; // Drag your EnemyBase prefab here
    [SerializeField] private GameObject laserDronePrefab;    // Drag your new LaserDrone prefab here
    [SerializeField] private float spawnXPosition = 9f; 
    [SerializeField] private float minY = -4f;         
    [SerializeField] private float maxY = 4f;          
    [SerializeField] private float timeBetweenWaves = 3f;

    private int currentWaveNumber = 1;
    private bool isWaveActive = false;
    private int enemiesRemainingAlive = 0;
    private int totalEnemiesThisWave = 0;

    private void Start()
    {
        if (standardEnemyPrefab == null)
        {
            Debug.LogError("[CoreDefender] Standard Enemy Prefab is missing in WaveManager!");
            return;
        }

        StartCoroutine(StartNextWaveRoutine());
    }

    private IEnumerator StartNextWaveRoutine()
    {
        isWaveActive = true;

        totalEnemiesThisWave = Mathf.RoundToInt(5 + (currentWaveNumber * 3)); 
        float spawnInterval = Mathf.Max(0.3f, 1.5f - (currentWaveNumber * 0.08f)); 
        float healthMultiplier = 1f + (currentWaveNumber * 0.2f); 
    

        enemiesRemainingAlive = totalEnemiesThisWave;

        string waveCategory = currentWaveNumber <= 3 ? "Easy" : (currentWaveNumber <= 8 ? "Hard" : "Nightmare");
        Debug.Log($"[CoreDefender] Starting Wave {currentWaveNumber} [{waveCategory}] - Enemies: {totalEnemiesThisWave}");

        yield return new WaitForSeconds(1.5f); 

        for (int i = 0; i < totalEnemiesThisWave; i++)
        {
            SpawnEnemy(healthMultiplier);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy(float healthMultiplier)
    {
        // From Wave 3 onwards, introduce a 30% chance to spawn a Laser Drone instead of a standard ground/air unit
        bool spawnDrone = currentWaveNumber >= 3 && laserDronePrefab != null && Random.value < 0.3f;
        GameObject prefabToSpawn = spawnDrone ? laserDronePrefab : standardEnemyPrefab;

        if (prefabToSpawn != null)
        {
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(spawnXPosition, randomY, 0f);

            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }

    public void NotifyEnemyDefeated()
    {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive <= 0 && isWaveActive)
        {
            isWaveActive = false;
            Debug.Log($"[CoreDefender] Wave {currentWaveNumber} Cleared! Preparing next wave...");
            currentWaveNumber++;
            StartCoroutine(WaveTransitionRoutine());
        }
    }

    private IEnumerator WaveTransitionRoutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartNextWaveRoutine());
    }
}