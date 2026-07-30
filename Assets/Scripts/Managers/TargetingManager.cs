using UnityEngine;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header("Dispatch Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    private float updateTimer = 0f;

    private Dictionary<TurretController, Transform> turretAssignments = new Dictionary<TurretController, Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            CleanAssignments();
            updateTimer = updateInterval;
        }
    }

    public Transform GetAssignedTarget(TurretController turret, float attackRange)
    {
        if (turretAssignments.TryGetValue(turret, out Transform currentTarget) && currentTarget != null)
        {
            float dist = Vector3.Distance(turret.transform.position, currentTarget.position);
            if (dist <= attackRange)
            {
                return currentTarget;
            }
        }

        Transform newTarget = CalculateSmartTarget(turret.transform, attackRange);
        turretAssignments[turret] = newTarget;
        return newTarget;
    }

    private Transform CalculateSmartTarget(Transform turretTransform, float attackRange)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        Dictionary<Transform, int> targetCounts = new Dictionary<Transform, int>();
        foreach (var kvp in turretAssignments)
        {
            if (kvp.Value != null)
            {
                if (!targetCounts.ContainsKey(kvp.Value)) targetCounts[kvp.Value] = 0;
                targetCounts[kvp.Value]++;
            }
        }

        Transform bestEnemy = null;
        float bestScore = float.MinValue;

        foreach (GameObject enemyObj in enemies)
        {
            if (enemyObj == null) continue;
            Transform enemy = enemyObj.transform;

            float dist = Vector3.Distance(turretTransform.position, enemy.position);
            if (dist > attackRange) continue; 

            float coreProximityScore = -enemy.position.x; 
            int currentAssignedTurrets = targetCounts.ContainsKey(enemy) ? targetCounts[enemy] : 0;
            
            float totalScore = coreProximityScore - (currentAssignedTurrets * 5f);

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestEnemy = enemy;
            }
        }

        if (bestEnemy == null)
        {
            float shortestDist = float.MaxValue;
            foreach (GameObject enemyObj in enemies)
            {
                if (enemyObj == null) continue;
                float dist = Vector3.Distance(turretTransform.position, enemyObj.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    bestEnemy = enemyObj.transform;
                }
            }
        }

        return bestEnemy;
    }

    private void CleanAssignments()
    {
        List<TurretController> keys = new List<TurretController>(turretAssignments.Keys);
        foreach (var turret in keys)
        {
            if (turret == null || turretAssignments[turret] == null)
            {
                turretAssignments.Remove(turret);
            }
        }
    }
}