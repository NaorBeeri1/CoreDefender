using UnityEngine;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header("Dispatch Settings")]
    [SerializeField] private float updateInterval = 0.15f;
    private float updateTimer = 0f;

    private Dictionary<TurretController, Transform> turretAssignments = new Dictionary<TurretController, Transform>();
    private Dictionary<Transform, int> pendingDamageMap = new Dictionary<Transform, int>();

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
        return null;
    }

    public void RegisterBulletFired(Transform target, int damage)
    {
        if (target != null)
        {
            if (!pendingDamageMap.ContainsKey(target))
                pendingDamageMap[target] = 0;
            
            pendingDamageMap[target] += damage;
        }
    }

    // Called by ProjectileController the moment a bullet hits its target
    public void NotifyBulletHit(Transform target, int damage)
    {
        if (target != null && pendingDamageMap.ContainsKey(target))
        {
            pendingDamageMap[target] = Mathf.Max(0, pendingDamageMap[target] - damage);
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
            pendingDamageMap.Clear();
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
        activeEnemies.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        turretAssignments.Clear();

        foreach (TurretController turret in availableTurrets)
        {
            Transform bestTarget = null;
            TurretData data = turret.GetTurretData();
            int turretDamage = data != null ? data.damage : 50;

            foreach (Transform enemy in activeEnemies)
            {
                int enemyMaxHP = GetEnemyMaxHealth(enemy);
                int incomingDamage = pendingDamageMap.ContainsKey(enemy) ? pendingDamageMap[enemy] : 0;

                // Only assign if incoming mid-air damage hasn't fully covered the enemy's HP yet
                if (incomingDamage < enemyMaxHP)
                {
                    bestTarget = enemy;
                    if (!pendingDamageMap.ContainsKey(enemy)) pendingDamageMap[enemy] = 0;
                    pendingDamageMap[enemy] += turretDamage;
                    break;
                }
            }

            if (bestTarget != null)
            {
                turretAssignments[turret] = bestTarget;
            }
        }
    }

    private int GetEnemyMaxHealth(Transform enemyTransform)
    {
        LaserDroneController drone = enemyTransform.GetComponent<LaserDroneController>();
        if (drone != null) return 150;

        return 100; // Standard enemy HP
    }

    private void CleanCollections()
    {
        List<Transform> damageKeys = new List<Transform>(pendingDamageMap.Keys);
        foreach (var target in damageKeys)
        {
            if (target == null)
            {
                pendingDamageMap.Remove(target);
            }
        }
    }
}