using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TurretController : MonoBehaviour
{
    [Header("Data Profile")]
    [SerializeField] private TurretData turretData;

    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject turretCanvasPrefab; // Heat bar canvas
    private GameObject activeHeatCanvas;
    private Image fillBarImage;

    private float currentHeat = 0f;
    private float fireCooldown = 0f;
    private bool isOverheated = false;
    private Transform currentTarget;
    private bool isUpgraded = false;

    private void Start()
    {
        // Instantiate the floating heat bar tightly above the turret head
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

        // Note: When game is paused (Time.timeScale == 0), Update stops executing. 
        // We use unscaled time checks or allow mouse raycasts during pause for UI selection.
        HandleCooling();
        FindNearestEnemy();

        fireCooldown -= Time.unscaledDeltaTime;
        
        if (currentTarget != null && fireCooldown <= (1f / turretData.fireRate) && !isOverheated)
        {
            // Only shoot if game is active
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
        // Allow clicking even when paused (Time.timeScale == 0) by checking Input directly
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

    public bool IsUpgraded() => isUpgraded;
    public int GetUpgradeCost() => turretData != null ? turretData.upgradeCost : 150;

    public void ExecuteUpgrade()
    {
        if (isUpgraded || turretData == null) return;

        if (PlayerStats.Instance != null && PlayerStats.Instance.SpendCredits(turretData.upgradeCost))
        {
            isUpgraded = true;
            turretData.fireRate *= turretData.fireRateMultiplier;
            turretData.damage += turretData.damageUpgradeBonus;

            Debug.Log($"[CoreDefender] {turretData.turretName} upgraded! New Fire Rate: {turretData.fireRate}, New Damage: {turretData.damage}");
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
            }
        }

        currentHeat += turretData.heatPerShot;

        if (currentHeat >= turretData.maxHeat)
        {
            isOverheated = true;
            Debug.LogWarning($"[CoreDefender] {turretData.turretName} OVERHEATED!");
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
                if (isOverheated)
                {
                    isOverheated = false;
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