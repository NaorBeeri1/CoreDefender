using UnityEngine;
using UnityEngine.UI;

public class TurretController : MonoBehaviour
{
    [Header("Data Profile")]
    [SerializeField] private TurretData turretData; // Drag StandardTurretData here

    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject turretCanvasPrefab; 
    private GameObject activeCanvasInstance;
    private Image fillBarImage;

    private float currentHeat = 0f;
    private float fireCooldown = 0f;
    private bool isOverheated = false;
    private Transform currentTarget;

    private void Start()
    {
        // Instantiate the floating heat bar tightly above the turret head (offset Y = 0.65f)
        if (turretCanvasPrefab != null)
        {
            activeCanvasInstance = Instantiate(turretCanvasPrefab, transform.position + new Vector3(0f, 0.65f, 0f), Quaternion.identity, transform);
            
            Transform fillTrans = activeCanvasInstance.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null)
            {
                fillBarImage = fillTrans.GetComponent<Image>();
            }
        }
    }

    private void Update()
    {
        if (turretData == null) return;

        HandleCooling();
        FindNearestEnemy();

        fireCooldown -= Time.deltaTime;
        
        if (currentTarget != null && fireCooldown <= 0f && !isOverheated)
        {
            Shoot();
            fireCooldown = 1f / turretData.fireRate;
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

        if (nearestEnemy != null && shortestDistance <= turretData.attackRange)
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

        // Use Object Pooling instead of Instantiate()
        GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool("Bullet", transform.position, Quaternion.identity);
        if (bulletObj != null)
        {
            ProjectileController projectile = bulletObj.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.SetTarget(currentTarget);
            }
        }

        currentHeat += turretData.heatPerShot;
        Debug.Log($"[CoreDefender] {turretData.turretName} fired! Current Heat: {currentHeat}/{turretData.maxHeat}");

        if (currentHeat >= turretData.maxHeat)
        {
            isOverheated = true;
            Debug.LogWarning($"[CoreDefender] {turretData.turretName} OVERHEATED! Cooling required.");
        }
    }

    private void HandleCooling()
    {
        if (currentHeat > 0f)
        {
            currentHeat -= turretData.coolingRate * Time.deltaTime;
            if (currentHeat <= 0f)
            {
                currentHeat = 0f;
                if (isOverheated)
                {
                    isOverheated = false;
                    Debug.Log($"[CoreDefender] {turretData.turretName} cooled down. Operational again.");
                }
            }
        }
    }

    private void UpdateHeatUI()
    {
        if (fillBarImage != null && turretData != null)
        {
            fillBarImage.fillAmount = currentHeat / turretData.maxHeat;

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
        if (turretData != null)
        {
            Gizmos.color = new Color(1f, 0f, 0.3f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, turretData.attackRange);
        }
    }
}