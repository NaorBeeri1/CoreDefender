using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveAnnouncementText; 
    [SerializeField] private TextMeshProUGUI waveCounterText;     
    [SerializeField] private TextMeshProUGUI waveCountdownText;   
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;

    [Header("Wave Base Settings")]
    [SerializeField] private GameObject standardEnemyPrefab; 
    [SerializeField] private GameObject laserDronePrefab;    
    [SerializeField] private float spawnXPosition = 9f; 
    [SerializeField] private float minY = -4f;         
    [SerializeField] private float maxY = 4f;          
    [SerializeField] private float timeBetweenWaves = 10f; 

    private int currentWaveNumber = 1;
    private bool isWaveActive = false;
    private int totalEnemiesThisWave = 0;
    private int enemiesDefeatedThisWave = 0;
    private float currentCountdownTimer = 0f;
    private bool isSpawningWave = false;

    private void Start()
    {
        if (standardEnemyPrefab == null)
        {
            Debug.LogError("[CoreDefender] Standard Enemy Prefab is missing in WaveManager!");
            return;
        }

        UpdateUI();
        StartCoroutine(WaveCountdownRoutine());
    }

    private void Update()
    {
        if (isWaveActive && !isSpawningWave)
        {
            GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

            // If all physical enemies are gone from the screen, immediately clear the wave!
            if (remainingEnemies.Length == 0)
            {
                AdvanceToNextWave();
            }
        }
        
        UpdateUI(); // Keep UI updated continuously for live counts
    }

    private IEnumerator WaveCountdownRoutine()
    {
        currentCountdownTimer = timeBetweenWaves;

        while (currentCountdownTimer > 0f)
        {
            if (waveCountdownText != null)
            {
                waveCountdownText.gameObject.SetActive(true);
                waveCountdownText.text = $"Next Wave in: {Mathf.Ceil(currentCountdownTimer)}s";
            }

            currentCountdownTimer -= Time.deltaTime;
            yield return null;
        }

        if (waveCountdownText != null)
        {
            waveCountdownText.text = "";
            waveCountdownText.gameObject.SetActive(false);
        }

        StartCoroutine(StartNextWaveRoutine());
    }

    private IEnumerator StartNextWaveRoutine()
    {
        isWaveActive = true;
        isSpawningWave = true;
        
        totalEnemiesThisWave = Mathf.RoundToInt(5 + (currentWaveNumber * 3));
        enemiesDefeatedThisWave = 0;

        float spawnInterval = Mathf.Max(0.35f, 2.0f - (currentWaveNumber * 0.08f)); 
        float healthMultiplier = 1f + (currentWaveNumber * 0.2f); 

        int spawnedCount = 0;
        while (spawnedCount < totalEnemiesThisWave)
        {
            int enemiesToSpawnNow = Random.Range(1, 4);

            for (int i = 0; i < enemiesToSpawnNow && spawnedCount < totalEnemiesThisWave; i++)
            {
                SpawnEnemy(healthMultiplier);
                spawnedCount++;
            }

            UpdateUI();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawningWave = false; 
    }

    private void SpawnEnemy(float healthMultiplier)
    {
        bool spawnDrone = currentWaveNumber >= 3 && laserDronePrefab != null && Random.value < 0.3f;
        GameObject prefabToSpawn = spawnDrone ? laserDronePrefab : standardEnemyPrefab;

        if (prefabToSpawn != null)
        {
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(spawnXPosition, randomY, 0f);

            GameObject enemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            enemy.tag = "Enemy"; 
        }
    }

    public void NotifyEnemyDefeated()
    {
        enemiesDefeatedThisWave++;
        UpdateUI();
    }

    private void AdvanceToNextWave()
    {
        if (!isWaveActive) return;

        isWaveActive = false;
        currentWaveNumber++;
        StartCoroutine(WaveCountdownRoutine());
    }

    private void UpdateUI()
    {
        if (waveCounterText != null)
        {
            waveCounterText.text = $"Wave: {currentWaveNumber}";
        }
        if (enemiesRemainingText != null)
        {
            // Total enemies left in this wave = Total scheduled minus those already defeated
            int enemiesLeft = Mathf.Max(0, totalEnemiesThisWave - enemiesDefeatedThisWave);
            
            // Failsafe backup: if physical count on screen is higher (due to edge cases), use that instead
            GameObject[] physicalEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            int finalCount = Mathf.Max(enemiesLeft, physicalEnemies.Length);

            enemiesRemainingText.text = $"Enemies: {finalCount}";
        }
    }
}