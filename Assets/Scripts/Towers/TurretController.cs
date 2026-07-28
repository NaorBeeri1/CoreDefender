using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI References")]
    [SerializeField] private GameObject turretCanvasPrefab; // Drag TurretCanvas prefab here
    private GameObject activeCanvasInstance;
    private Image fillBarImage;

    private float currentHeat = 0f;
    private float fireCooldown = 0f;
    private bool isOverheated = false;
    private Transform currentTarget;

    private void Start()
    {
        // Instantiate the floating heat bar canvas above the turret
        if (turretCanvasPrefab != null)
        {
            activeCanvasInstance = Instantiate(turretCanvasPrefab, transform.position + new Vector3(0f, 1.2f, 0f), Quaternion.identity, transform);
            
            // Find the FillBar image inside the instantiated canvas
            Transform fillTrans = activeCanvasInstance.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null)
            {
                fillBarImage = fillTrans.GetComponent<Image>();
            }
        }
    }

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

        UpdateHeatUI();
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

    private void UpdateHeatUI()
    {
        if (fillBarImage != null)
        {
            // Update fill amount based on current heat percentage
            fillBarImage.fillAmount = currentHeat / maxHeat;

            // Change color to bright red/pink (#FF1A4D) if overheated, or cyan (#1AE6FF) when normal
            if (isOverheated)
            {
                fillBarImage.color = new Color(1f, 0.1f, 0.302f, 1f); // #FF1A4D
            }
            else
            {
                fillBarImage.color = new Color(0.102f, 0.902f, 1f, 1f); // #1AE6FF
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}