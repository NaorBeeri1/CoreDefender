using UnityEngine;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header("Dispatch Settings")]
    [SerializeField] private float updateInterval = 0.15f;
    private float updateTimer = 0f;

    private Dictionary<TurretController, Transform> turretAssignments = new Dictionary<TurretController, Transform>();
    private Dictionary<Transform, int> enemyAssignedBulletCounts = new Dictionary<Transform, int>();

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
            UpdateAssignments();
            updateTimer = updateInterval;
        }
    }

    public Transform GetAssignedTarget(TurretController turret)
    {
        if (turretAssignments.TryGetValue(turret, out Transform target))
        {
            return target;
        }
        return null;
    }

    public void RegisterBulletFired(Transform target)
    {
        if (target != null)
        {
            if (!enemyAssignedBulletCounts.ContainsKey(target))
                enemyAssignedBulletCounts[target] = 0;
            
            enemyAssignedBulletCounts[target]++;
        }
    }

    private void UpdateAssignments()
    {
        CleanCollections();

        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] turretObjs = GameObject.FindGameObjectsWithTag("Turret");

        if (enemyObjs.Length == 0 || turretObjs.Length == 0)
        {
            turretAssignments.Clear();
            return;
        }

        // Shuffle turrets randomly
        List<TurretController> availableTurrets = new List<TurretController>();
        foreach (GameObject tObj in turretObjs)
        {
            TurretController tc = tObj.GetComponent<TurretController>();
            if (tc != null) availableTurrets.Add(tc);
        }

        for (int i = 0; i < availableTurrets.Count; i++)
        {
            TurretController temp = availableTurrets[i];
            int randomIndex = Random.Range(i, availableTurrets.Count);
            availableTurrets[i] = availableTurrets[randomIndex];
            availableTurrets[randomIndex] = temp;
        }

        // Sort enemies by X position (closest to core first)
        List<Transform> activeEnemies = new List<Transform>();
        foreach (GameObject eObj in enemyObjs)
        {
            if (eObj != null) activeEnemies.Add(eObj.transform);
        }
        activeEnemies.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        turretAssignments.Clear();

        // Assign turrets to enemies globally without any range restrictions
        foreach (TurretController turret in availableTurrets)
        {
            Transform bestTarget = null;

            foreach (Transform enemy in activeEnemies)
            {
                int currentAssignedBullets = enemyAssignedBulletCounts.ContainsKey(enemy) ? enemyAssignedBulletCounts[enemy] : 0;
                int maxAllowedBullets = GetRequiredBulletQuota(enemy);

                if (currentAssignedBullets < maxAllowedBullets)
                {
                    bestTarget = enemy;
                    enemyAssignedBulletCounts[enemy] = currentAssignedBullets + 1;
                    break;
                }
            }

            if (bestTarget != null)
            {
                turretAssignments[turret] = bestTarget;
            }
        }
    }

    private int GetRequiredBulletQuota(Transform enemyTransform)
    {
        LaserDroneController drone = enemyTransform.GetComponent<LaserDroneController>();
        if (drone != null) return 3; // 150 HP drone needs 3 bullets

        return 2; // Standard enemy needs 2 bullets
    }

    private void CleanCollections()
    {
        List<Transform> targetKeys = new List<Transform>(enemyAssignedBulletCounts.Keys);
        foreach (var target in targetKeys)
        {
            if (target == null)
            {
                enemyAssignedBulletCounts.Remove(target);
            }
        }
    }
}