using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "CoreDefender/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Identification")]
    public string waveName = "Wave 1";

    [Header("Spawn Parameters")]
    public GameObject enemyPrefab;    
    public int enemyCount = 10;       
    public float spawnInterval = 1.2f; 

    [Header("Difficulty Scaling")]
    public int healthBonusPerEnemy = 20; // Makes subsequent waves tougher!
}