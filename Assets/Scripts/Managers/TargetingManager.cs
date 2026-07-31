using UnityEngine;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header("Dispatch Settings")]
    [SerializeField] private float updateInterval = 0.05f;
    private float updateTimer = 0f;

    private Dictionary<TurretController, Transform> turretAssignments = new Dictionary<TurretController, Transform>();
    private Dictionary<Transform, int> inFlightDamage = new Dictionary<Transform, int>();

    private class TurretInfo
    {
        public TurretController turret;
        public int damage;
        public bool isCryo;
    }

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
        if (turret != null && turretAssignments.TryGetValue(turret, out Transform target) && target != null)
        {
            return target;
        }
        return null;
    }

    public void RegisterBulletFired(Transform target, int damage)
    {
        if (target == null) return;
        if (!inFlightDamage.ContainsKey(target)) inFlightDamage[target] = 0;
        inFlightDamage[target] += damage;
    }

    public void NotifyBulletHit(Transform target, int damage)
    {
        if (target == null) return;
        if (inFlightDamage.ContainsKey(target))
        {
            inFlightDamage[target] -= damage;
            if (inFlightDamage[target] < 0) inFlightDamage[target] = 0;
        }
    }

    private void UpdateAssignments()
    {
        CleanCollections();

        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] turretObjs = GameObject.FindGameObjectsWithTag("Turret");

        turretAssignments.Clear();

        if (enemyObjs.Length == 0 || turretObjs.Length == 0) return;

        List<TurretInfo> availableTurrets = new List<TurretInfo>();
        
        foreach (GameObject tObj in turretObjs)
        {
            if (tObj == null) continue;
            TurretController tc = tObj.GetComponent<TurretController>();
            if (tc != null) availableTurrets.Add(new TurretInfo { turret = tc, damage = tc.GetTurretData().damage, isCryo = tc.IsCryo() });
        }

        // Heavy hitters lock onto fresh targets first
        availableTurrets.Sort((a, b) => b.damage.CompareTo(a.damage));

        List<Transform> activeEnemies = new List<Transform>();
        foreach (GameObject eObj in enemyObjs)
        {
            if (eObj != null) activeEnemies.Add(eObj.transform);
        }
        
        // Closest to the core gets priority
        activeEnemies.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        Dictionary<Transform, int> projectedDamage = new Dictionary<Transform, int>();
        foreach (var kvp in inFlightDamage)
        {
            if (kvp.Key != null) projectedDamage[kvp.Key] = kvp.Value;
        }

        foreach (TurretInfo tInfo in availableTurrets)
        {
            Transform chosenEnemy = null;
            Transform fallbackFrozenEnemy = null;

            foreach (Transform enemy in activeEnemies)
            {
                if (enemy == null) continue;

                int currentHp = GetEnemyHealth(enemy);
                int incomingDmg = projectedDamage.ContainsKey(enemy) ? projectedDamage[enemy] : 0;

                // TACTICAL CHECK: Is the enemy still mathematically alive?
                if (currentHp - incomingDmg > 0)
                {
                    if (tInfo.isCryo)
                    {
                        // Prioritize unfrozen. If frozen, save as fallback and keep looking.
                        if (IsEnemyFrozen(enemy))
                        {
                            if (fallbackFrozenEnemy == null) fallbackFrozenEnemy = enemy;
                            continue;
                        }
                    }
                    
                    chosenEnemy = enemy;
                    break;
                }
            }

            // CRYO FALLBACK: If all living enemies are already frozen, shoot the closest frozen one to keep doing damage
            if (chosenEnemy == null && tInfo.isCryo && fallbackFrozenEnemy != null)
            {
                chosenEnemy = fallbackFrozenEnemy;
            }

            // GENERAL FALLBACK: If all enemies are marked for death, shoot the closest body to prevent idling
            if (chosenEnemy == null)
            {
                chosenEnemy = activeEnemies[0];
            }

            if (chosenEnemy != null)
            {
                turretAssignments[tInfo.turret] = chosenEnemy;
                if (!projectedDamage.ContainsKey(chosenEnemy)) projectedDamage[chosenEnemy] = 0;
                
                projectedDamage[chosenEnemy] += tInfo.damage;
            }
        }
    }

    private int GetEnemyHealth(Transform enemy)
    {
        if (enemy == null) return 0;
        EnemyContext ctx = enemy.GetComponent<EnemyContext>();
        if (ctx != null) return ctx.GetCurrentHealth();
        
        LaserDroneController drone = enemy.GetComponent<LaserDroneController>();
        if (drone != null) return drone.GetCurrentHealth();

        EnemyController ctrl = enemy.GetComponent<EnemyController>();
        if (ctrl != null) return ctrl.GetCurrentHealth();

        return 100;
    }

    private bool IsEnemyFrozen(Transform enemy)
    {
        if (enemy == null) return false;
        EnemyContext ctx = enemy.GetComponent<EnemyContext>();
        if (ctx != null) return ctx.IsFrozen();
        
        LaserDroneController drone = enemy.GetComponent<LaserDroneController>();
        if (drone != null) return drone.IsFrozen();

        EnemyController ctrl = enemy.GetComponent<EnemyController>();
        if (ctrl != null) return ctrl.IsFrozen();

        return false;
    }

    private void CleanCollections()
    {
        List<TurretController> keys = new List<TurretController>(turretAssignments.Keys);
        foreach (var key in keys)
        {
            if (key == null || turretAssignments[key] == null) turretAssignments.Remove(key);
        }

        List<Transform> inFlightKeys = new List<Transform>(inFlightDamage.Keys);
        foreach (var key in inFlightKeys)
        {
            if (key == null) inFlightDamage.Remove(key);
        }
    }
}