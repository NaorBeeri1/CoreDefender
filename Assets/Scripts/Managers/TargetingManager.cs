using UnityEngine;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header("Dispatch Settings")]
    [SerializeField] private float updateInterval = 0.05f;
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
            UpdateAssignments();
            updateTimer = updateInterval;
        }
    }

    public Transform GetAssignedTarget(TurretController turret, float attackRange = 0f)
    {
        if (turretAssignments.TryGetValue(turret, out Transform target) && target != null)
        {
            return target;
        }
        
        return FindBestTargetForTurret(turret);
    }

    public void RegisterBulletFired(Transform target, int damage)
    {
        // Not needed with direct assignment limiting, but kept for compatibility
    }

    public void NotifyBulletHit(Transform target, int damage)
    {
        // Not needed with direct assignment limiting, but kept for compatibility
    }

    private void UpdateAssignments()
    {
        CleanCollections();

        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] turretObjs = GameObject.FindGameObjectsWithTag("Turret");

        turretAssignments.Clear();

        if (enemyObjs.Length == 0 || turretObjs.Length == 0)
        {
            return;
        }

        List<TurretController> availableTurrets = new List<TurretController>();
        foreach (GameObject tObj in turretObjs)
        {
            TurretController tc = tObj.GetComponent<TurretController>();
            if (tc != null) availableTurrets.Add(tc);
        }

        // Prioritize higher damage turrets first
        availableTurrets.Sort((a, b) => b.GetTurretData().damage.CompareTo(a.GetTurretData().damage));

        List<Transform> activeEnemies = new List<Transform>();
        foreach (GameObject eObj in enemyObjs)
        {
            if (eObj != null) activeEnemies.Add(eObj.transform);
        }
        // Sort strictly by proximity to core (lowest X position first)
        activeEnemies.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        // Track how many turrets are currently assigned to each enemy
        Dictionary<Transform, int> enemyAssignmentCounts = new Dictionary<Transform, int>();

        foreach (TurretController turret in availableTurrets)
        {
            Transform chosenEnemy = null;

            // Find the closest enemy that has fewer than 2 turrets targeting it (assuming 2 bullets/turrets are enough to kill a 100 HP enemy with 50 damage)
            foreach (Transform enemy in activeEnemies)
            {
                if (enemy == null) continue;

                int assignedCount = enemyAssignmentCounts.ContainsKey(enemy) ? enemyAssignmentCounts[enemy] : 0;

                // Limit to 2 turrets per enemy at a time to prevent unnecessary overkills
                if (assignedCount < 2)
                {
                    chosenEnemy = enemy;
                    break;
                }
            }

            // Fallback: if all enemies already have 2 turrets, target the absolute closest one anyway
            if (chosenEnemy == null && activeEnemies.Count > 0)
            {
                chosenEnemy = activeEnemies[0];
            }

            if (chosenEnemy != null)
            {
                turretAssignments[turret] = chosenEnemy;
                if (!enemyAssignmentCounts.ContainsKey(chosenEnemy)) enemyAssignmentCounts[chosenEnemy] = 0;
                enemyAssignmentCounts[chosenEnemy]++;
            }
        }
    }

    private Transform FindBestTargetForTurret(TurretController turret)
    {
        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyObjs.Length == 0) return null;

        Transform closest = null;
        float minX = float.MaxValue;

        foreach (GameObject e in enemyObjs)
        {
            if (e != null && e.transform.position.x < minX)
            {
                minX = e.transform.position.x;
                closest = e.transform;
            }
        }
        return closest ?? enemyObjs[0].transform;
    }

    private void CleanCollections()
    {
        List<TurretController> keys = new List<TurretController>(turretAssignments.Keys);
        foreach (var key in keys)
        {
            if (key == null || turretAssignments[key] == null)
            {
                turretAssignments.Remove(key);
            }
        }
    }
}