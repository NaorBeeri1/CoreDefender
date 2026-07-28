using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "CoreDefender/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Identification")]
    public string waveName = "Wave 1";

    [Header("Spawn Parameters")]
    public GameObject enemyPrefab;    // Can be standard or specialized enemy
    public int enemyCount = 10;       // Total enemies in this wave
    public float spawnInterval = 1.5f; // Delay between each spawn
}