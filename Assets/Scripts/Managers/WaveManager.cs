using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveAnnouncementText; 
    [SerializeField] private TextMeshProUGUI waveCounterText;     
    [SerializeField] private TextMeshProUGUI waveCountdownText;   
    [SerializeField] private TextMeshProUGUI enemiesRemainingText; // <-- Added UI link

    [Header("Wave Base Settings")]
    [SerializeField] private GameObject standardEnemyPrefab; 
    [SerializeField] private GameObject laserDronePrefab;    
    [SerializeField] private float spawnXPosition = 9f; 
    [SerializeField] private float minY = -4f;         
    [SerializeField] private float maxY = 4f;          
    [SerializeField] private float timeBetweenWaves = 10f; 

    private int currentWaveNumber = 1;
    private bool isWaveActive = false;
    private int enemiesRemainingAlive = 0;
    private int totalEnemiesThisWave = 0;
    private float currentCountdownTimer = 0f;

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
        totalEnemiesThisWave = Mathf.RoundToInt(5 + (currentWaveNumber * 3)); 
        float spawnInterval = Mathf.Max(0.3f, 1.5f - (currentWaveNumber * 0.08f)); 
        float healthMultiplier = 1f + (currentWaveNumber * 0.2f); 

        enemiesRemainingAlive = totalEnemiesThisWave;
        UpdateUI();

        for (int i = 0; i < totalEnemiesThisWave; i++)
        {
            SpawnEnemy(healthMultiplier);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy(float healthMultiplier)
    {
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
        enemiesRemainingAlive = Mathf.Max(0, enemiesRemainingAlive - 1);
        UpdateUI();

        if (enemiesRemainingAlive <= 0 && isWaveActive)
        {
            isWaveActive = false;
            currentWaveNumber++;
            StartCoroutine(WaveCountdownRoutine());
        }
    }

    private void UpdateUI()
    {
        if (waveCounterText != null)
        {
            waveCounterText.text = $"Wave: {currentWaveNumber}";
        }
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = $"Enemies: {enemiesRemainingAlive}";
        }
    }
}