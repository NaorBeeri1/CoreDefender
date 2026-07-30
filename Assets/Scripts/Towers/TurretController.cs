using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class TurretController : MonoBehaviour
{
    [Header("Data Profiles (Assign either TurretData or SniperTurretData)")]
    [SerializeField] private ScriptableObject turretDataAsset; 
    
    // Runtime properties unified
    private string turretName = "Turret";
    private float fireRate = 1f;
    private float attackRange = 5f;
    private int damage = 50;
    private float maxHeat = 100f;
    private float heatPerShot = 15f;
    private float coolingRate = 40f;
    private int currentUpgradeLevel = 0;
    private int maxUpgradeLevel = 3;
    private int upgradeCost = 150;
    private float fireRateMultiplier = 1.2f;
    private int damageUpgradeBonus = 25;
    private int healthUpgradeBonus = 25;
    private float coolingRateMultiplier = 1.5f;
    private Sprite[] upgradeSprites;

    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer; 

    [Header("UI References")]
    [SerializeField] private GameObject turretCanvasPrefab;
    private GameObject activeHeatCanvas;
    private Image fillBarImage;
    private TextMeshProUGUI turretHpText; 

    [Header("Turret Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private float currentHeat = 0f;
    private float fireCooldown = 0f;
    private bool isOverheated = false;
    private Transform currentTarget;

    private void Start()
    {
        InitializeData();
        currentHealth = maxHealth;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateTurretVisuals();

        if (turretCanvasPrefab != null)
        {
            activeHeatCanvas = Instantiate(turretCanvasPrefab, transform.position + new Vector3(0f, 0.65f, 0f), Quaternion.identity, transform);
            
            Transform fillTrans = activeHeatCanvas.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null)
            {
                fillBarImage = fillTrans.GetComponent<Image>();
            }

            Transform hpTrans = activeHeatCanvas.transform.Find("BackgroundBar/TurretHPText");
            if (hpTrans == null)
            {
                GameObject hpTextObj = new GameObject("TurretHPText", typeof(RectTransform), typeof(TextMeshProUGUI));
                hpTextObj.transform.SetParent(activeHeatCanvas.transform.Find("BackgroundBar"), false);
                
                RectTransform rt = hpTextObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, 15f);
                rt.sizeDelta = new Vector2(100f, 20f);

                turretHpText = hpTextObj.GetComponent<TextMeshProUGUI>();
                turretHpText.fontSize = 12;
                turretHpText.alignment = TextAlignmentOptions.Center;
                turretHpText.color = Color.green;
            }
            else
            {
                turretHpText = hpTrans.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void InitializeData()
    {
        if (turretDataAsset is SniperTurretData sniperData)
        {
            turretName = sniperData.turretName;
            fireRate = sniperData.fireRate;
            attackRange = sniperData.attackRange;
            damage = sniperData.damage;
            maxHeat = sniperData.maxHeat;
            heatPerShot = sniperData.heatPerShot;
            coolingRate = sniperData.coolingRate;
            currentUpgradeLevel = sniperData.currentUpgradeLevel;
            maxUpgradeLevel = sniperData.maxUpgradeLevel;
            upgradeCost = sniperData.upgradeCost;
            fireRateMultiplier = sniperData.fireRateMultiplier;
            damageUpgradeBonus = sniperData.damageUpgradeBonus;
            healthUpgradeBonus = sniperData.healthUpgradeBonus;
            coolingRateMultiplier = sniperData.coolingRateMultiplier;
            upgradeSprites = sniperData.upgradeSprites;
        }
        else if (turretDataAsset is TurretData normalData)
        {
            turretName = normalData.turretName;
            fireRate = normalData.fireRate;
            attackRange = normalData.attackRange;
            damage = normalData.damage;
            maxHeat = normalData.maxHeat;
            heatPerShot = normalData.heatPerShot;
            coolingRate = normalData.coolingRate;
            currentUpgradeLevel = normalData.currentUpgradeLevel;
            maxUpgradeLevel = normalData.maxUpgradeLevel;
            upgradeCost = normalData.upgradeCost;
            fireRateMultiplier = normalData.fireRateMultiplier;
            damageUpgradeBonus = normalData.damageUpgradeBonus;
            healthUpgradeBonus = normalData.healthUpgradeBonus;
            coolingRateMultiplier = normalData.coolingRateMultiplier;
            upgradeSprites = normalData.upgradeSprites;
        }
        else
        {
            Debug.LogWarning($"[TurretController] No valid TurretData or SniperTurretData assigned to {gameObject.name}!");
        }
    }

    private void Update()
    {
        HandleCooling();

        if (TargetingManager.Instance != null)
        {
            currentTarget = TargetingManager.Instance.GetAssignedTarget(this, attackRange);
        }
        else
        {
            currentTarget = null;
        }

        fireCooldown -= Time.unscaledDeltaTime;
        
        if (currentTarget != null && fireCooldown <= 0f && !isOverheated)
        {
            if (Time.timeScale > 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }

        UpdateHeatAndHPUI();
        HandleSelectionInput();
    }

    public void TakeDamage(int incomingDamage)
    {
        currentHealth -= incomingDamage;
        if (currentHealth < 0) currentHealth = 0;

        if (currentHealth <= 0)
        {
            SellTurret(); 
        }
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

    // --- Runtime proxy structures for TargetingManager and TurretUIManager compatibility ---
    public class RuntimeTurretProxy
    {
        public int damage;
        public float fireRate;
        public float attackRange;
        public int currentUpgradeLevel;
        public int maxUpgradeLevel;
        public int upgradeCost;
        public float fireRateMultiplier;
        public int damageUpgradeBonus;
        public int healthUpgradeBonus;
    }

    public RuntimeTurretProxy GetTurretData()
    {
        return new RuntimeTurretProxy
        {
            damage = this.damage,
            fireRate = this.fireRate,
            attackRange = this.attackRange,
            currentUpgradeLevel = this.currentUpgradeLevel,
            maxUpgradeLevel = this.maxUpgradeLevel,
            upgradeCost = this.upgradeCost,
            fireRateMultiplier = this.fireRateMultiplier,
            damageUpgradeBonus = this.damageUpgradeBonus,
            healthUpgradeBonus = this.healthUpgradeBonus
        };
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    public void ExecuteUpgrade()
    {
        if (currentUpgradeLevel >= maxUpgradeLevel) return;

        if (PlayerStats.Instance != null && PlayerStats.Instance.SpendCredits(upgradeCost))
        {
            currentUpgradeLevel++;
            damage += damageUpgradeBonus; 
            fireRate *= fireRateMultiplier;
            coolingRate *= coolingRateMultiplier;

            maxHealth += healthUpgradeBonus;
            currentHealth += healthUpgradeBonus;

            UpdateTurretVisuals();
        }
    }

    private void UpdateTurretVisuals()
    {
        if (spriteRenderer != null && upgradeSprites != null)
        {
            int index = currentUpgradeLevel;
            if (index >= 0 && index < upgradeSprites.Length && upgradeSprites[index] != null)
            {
                spriteRenderer.sprite = upgradeSprites[index];
            }
        }
    }

    public void SellTurret()
    {
        int baseRefund = 75;
        int upgradeRefundBonus = 50;
        int totalRefund = baseRefund + (currentUpgradeLevel * upgradeRefundBonus);

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCredits(totalRefund);
        }
        Destroy(gameObject);
    }

    private void Shoot()
    {
        if (currentTarget == null) return;

        if (TargetingManager.Instance != null)
        {
            TargetingManager.Instance.RegisterBulletFired(currentTarget, damage);
        }

        GameObject bulletObj = ObjectPooler.Instance != null ? 
            ObjectPooler.Instance.SpawnFromPool("Bullet", transform.position, Quaternion.identity) : 
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        if (bulletObj != null)
        {
            ProjectileController projectile = bulletObj.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.SetTarget(currentTarget);
                projectile.SetDamage(damage);
            }
        }

        currentHeat += heatPerShot;
        if (currentHeat >= maxHeat)
        {
            isOverheated = true;
        }
    }

    private void HandleCooling()
    {
        if (currentHeat > 0f)
        {
            currentHeat -= coolingRate * Time.unscaledDeltaTime;
            if (currentHeat <= 0f)
            {
                currentHeat = 0f;
                if (isOverheated) isOverheated = false;
            }
        }
    }

    private void UpdateHeatAndHPUI()
    {
        if (fillBarImage != null)
        {
            fillBarImage.fillAmount = currentHeat / maxHeat;
            fillBarImage.color = isOverheated ? new Color(1f, 0.1f, 0.302f, 1f) : new Color(0.102f, 0.902f, 1f, 1f);
        }

        if (turretHpText != null)
        {
            turretHpText.text = $"{currentHealth} / {maxHealth} HP";
        }
    }
}