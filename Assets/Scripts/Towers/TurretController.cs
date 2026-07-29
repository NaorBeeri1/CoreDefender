using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TurretController : MonoBehaviour
{
    [Header("Data Profile")]
    [SerializeField] private TurretData originalTurretData; // Assign StandardTurretData here in Prefab
    private TurretData turretData;                           // Unique runtime instance copy

    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject turretCanvasPrefab;
    private GameObject activeHeatCanvas;
    private Image fillBarImage;

    private float currentHeat = 0f;
    private float fireCooldown = 0f;
    private bool isOverheated = false;
    private Transform currentTarget;

    private void Start()
    {
        if (originalTurretData != null)
        {
            // Instantiate a unique runtime copy so upgrades don't share across turrets
            turretData = Instantiate(originalTurretData);
            turretData.currentUpgradeLevel = 0;
            turretData.damage = 50; 
            turretData.fireRate = 1f;
        }

        if (turretCanvasPrefab != null)
        {
            activeHeatCanvas = Instantiate(turretCanvasPrefab, transform.position + new Vector3(0f, 0.65f, 0f), Quaternion.identity, transform);
            Transform fillTrans = activeHeatCanvas.transform.Find("BackgroundBar/FillBar");
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

        fireCooldown -= Time.unscaledDeltaTime;
        
        if (currentTarget != null && fireCooldown <= (1f / turretData.fireRate) && !isOverheated)
        {
            if (Time.timeScale > 0f)
            {
                Shoot();
                fireCooldown = 1f / turretData.fireRate;
            }
        }

        UpdateHeatUI();
        HandleSelectionInput();
    }

    private void HandleSelectionInput()
    {
        bool isClicked = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);

        if (isClicked && Time.timeScale >= 0f)
        {
            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            float distance = Vector3.Distance(transform.position, mouseWorldPos);
            if (distance <= 0.5f)
            {
                if (TurretUIManager.Instance != null)
                {
                    TurretUIManager.Instance.OpenMenu(this);
                }
            }
        }
    }

    public TurretData GetTurretData() => turretData;

    public void ExecuteUpgrade()
    {
        if (turretData == null || turretData.currentUpgradeLevel >= turretData.maxUpgradeLevel) return;

        if (PlayerStats.Instance != null && PlayerStats.Instance.SpendCredits(turretData.upgradeCost))
        {
            turretData.currentUpgradeLevel++;
            turretData.damage += turretData.damageUpgradeBonus; // Tier 1: 75, Tier 2: 100, Tier 3: 125
            turretData.fireRate *= turretData.fireRateMultiplier;
            turretData.coolingRate *= turretData.coolingRateMultiplier;

            Debug.Log($"[CoreDefender] Upgraded unique turret to Tier {turretData.currentUpgradeLevel}! Damage: {turretData.damage}");
        }
    }

    public void SellTurret()
    {
        int refundAmount = 75; 
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCredits(refundAmount);
        }
        Destroy(gameObject);
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

        GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool("Bullet", transform.position, Quaternion.identity);
        if (bulletObj != null)
        {
            ProjectileController projectile = bulletObj.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.SetTarget(currentTarget);
                projectile.SetDamage(turretData.damage);
            }
        }

        currentHeat += turretData.heatPerShot;
        if (currentHeat >= turretData.maxHeat)
        {
            isOverheated = true;
        }
    }

    private void HandleCooling()
    {
        if (currentHeat > 0f)
        {
            currentHeat -= turretData.coolingRate * Time.unscaledDeltaTime;
            if (currentHeat <= 0f)
            {
                currentHeat = 0f;
                if (isOverheated) isOverheated = false;
            }
        }
    }

    private void UpdateHeatUI()
    {
        if (fillBarImage != null && turretData != null)
        {
            fillBarImage.fillAmount = currentHeat / turretData.maxHeat;
            fillBarImage.color = isOverheated ? new Color(1f, 0.1f, 0.302f, 1f) : new Color(0.102f, 0.902f, 1f, 1f);
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