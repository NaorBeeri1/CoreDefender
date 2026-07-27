using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Turret Stats")]
    [SerializeField] private float fireRate = 1f; // Shots per second
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatPerShot = 15f;
    [SerializeField] private float coolingRate = 25f; // Heat lost per second when idle

    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;

    private float currentHeat = 0f;
    private float fireCooldown = 0f;
    private bool isOverheated = false;
    private Transform currentTarget;

    private void Update()
    {
        HandleCooling();
        FindNearestEnemy();

        fireCooldown -= Time.deltaTime;
        
        if (currentTarget != null && fireCooldown <= 0f && !isOverheated)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }

    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        if (nearestEnemy != null && shortestDistance <= attackRange)
        {
            currentTarget = nearestEnemy;
        }
        else
        {
            currentTarget = null;
        }
    }

    private void Shoot()
    {
        if (currentTarget == null) return;

        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            ProjectileController projectile = bulletObj.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.SetTarget(currentTarget);
            }
        }

        currentHeat += heatPerShot;
        Debug.Log($"[CoreDefender] Turret fired! Current Heat: {currentHeat}/{maxHeat}");

        if (currentHeat >= maxHeat)
        {
            isOverheated = true;
            Debug.LogWarning("[CoreDefender] TURRET OVERHEATED! Cooling required.");
        }
    }

    private void HandleCooling()
    {
        if (currentHeat > 0f)
        {
            currentHeat -= coolingRate * Time.deltaTime;
            if (currentHeat <= 0f)
            {
                currentHeat = 0f;
                if (isOverheated)
                {
                    isOverheated = false;
                    Debug.Log("[CoreDefender] Turret cooled down. Operational again.");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}