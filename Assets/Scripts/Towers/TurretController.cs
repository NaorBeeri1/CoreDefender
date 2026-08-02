using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class TurretController : MonoBehaviour
{
    [Header("Data Profiles (Assign TurretData, SniperData, or CryoData)")]
    [SerializeField] private ScriptableObject turretDataAsset; 
    
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

    private bool isCryo = false;

    [Header("Combat References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer; 

    [Header("UI & Health Bar Customization")]
    [SerializeField] private GameObject turretCanvasPrefab;
    [SerializeField] private Vector3 canvasOffset = new Vector3(0f, 0.6f, 0f); // Tweak height position here in Inspector
    [SerializeField] private Vector3 canvasScale = new Vector3(0.02f, 0.02f, 0.02f); // Tweak overall size here in Inspector
    [SerializeField] private Vector2 barSize = new Vector2(100f, 18f); // Tweak bar width/height here in Inspector

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

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateTurretVisuals();

        if (turretCanvasPrefab != null)
        {
            activeHeatCanvas = Instantiate(turretCanvasPrefab, transform.position + canvasOffset, Quaternion.identity, transform);
            activeHeatCanvas.transform.localScale = canvasScale;

            Transform bgTrans = activeHeatCanvas.transform.Find("BackgroundBar");
            if (bgTrans != null)
            {
                RectTransform bgRt = bgTrans.GetComponent<RectTransform>();
                bgRt.sizeDelta = barSize;
            }

            Transform fillTrans = activeHeatCanvas.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null) fillBarImage = fillTrans.GetComponent<Image>();

            Transform hpTrans = activeHeatCanvas.transform.Find("BackgroundBar/TurretHPText");
            if (hpTrans == null)
            {
                GameObject hpTextObj = new GameObject("TurretHPText", typeof(RectTransform), typeof(TextMeshProUGUI));
                hpTextObj.transform.SetParent(activeHeatCanvas.transform.Find("BackgroundBar"), false);
                
                RectTransform rt = hpTextObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(0f, 14f);
                rt.sizeDelta = new Vector2(120f, 25f);

                turretHpText = hpTextObj.GetComponent<TextMeshProUGUI>();
                turretHpText.fontSize = 13;
                turretHpText.alignment = TextAlignmentOptions.Center;
                turretHpText.color = Color.white;
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
            ApplyStats(sniperData.turretName, sniperData.fireRate, sniperData.attackRange, sniperData.damage, sniperData.maxHeat, sniperData.heatPerShot, sniperData.coolingRate, sniperData.currentUpgradeLevel, sniperData.maxUpgradeLevel, sniperData.upgradeCost, sniperData.fireRateMultiplier, sniperData.damageUpgradeBonus, sniperData.healthUpgradeBonus, sniperData.coolingRateMultiplier, sniperData.upgradeSprites);
        }
        else if (turretDataAsset is TurretData normalData)
        {
            ApplyStats(normalData.turretName, normalData.fireRate, normalData.attackRange, normalData.damage, normalData.maxHeat, normalData.heatPerShot, normalData.coolingRate, normalData.currentUpgradeLevel, normalData.maxUpgradeLevel, normalData.upgradeCost, normalData.fireRateMultiplier, normalData.damageUpgradeBonus, normalData.healthUpgradeBonus, normalData.coolingRateMultiplier, normalData.upgradeSprites);
        }
        else if (turretDataAsset is MassDriverData mdData)
        {
            ApplyStats(mdData.turretName, mdData.fireRate, mdData.attackRange, mdData.damage, mdData.maxHeat, mdData.heatPerShot, mdData.coolingRate, mdData.currentUpgradeLevel, mdData.maxUpgradeLevel, mdData.upgradeCost, mdData.fireRateMultiplier, mdData.damageUpgradeBonus, mdData.healthUpgradeBonus, mdData.coolingRateMultiplier, mdData.upgradeSprites);
        }
        else if (turretDataAsset is IonBeaconData ibData)
        {
            ApplyStats(ibData.turretName, ibData.fireRate, ibData.attackRange, ibData.damage, ibData.maxHeat, ibData.heatPerShot, ibData.coolingRate, ibData.currentUpgradeLevel, ibData.maxUpgradeLevel, ibData.upgradeCost, ibData.fireRateMultiplier, ibData.damageUpgradeBonus, ibData.healthUpgradeBonus, ibData.coolingRateMultiplier, ibData.upgradeSprites);
        }

        if (turretName.Contains("Cryo"))
        {
            isCryo = true;
            maxUpgradeLevel = 2;
        }
    }

    private void ApplyStats(string tName, float fRate, float aRange, int dmg, float mHeat, float hPerShot, float cRate, int cUpgrade, int mUpgrade, int uCost, float fMultiplier, int dBonus, int hBonus, float cMultiplier, Sprite[] sprites)
    {
        turretName = tName; fireRate = fRate; attackRange = aRange; damage = dmg; maxHeat = mHeat; heatPerShot = hPerShot; coolingRate = cRate; currentUpgradeLevel = cUpgrade; maxUpgradeLevel = mUpgrade; upgradeCost = uCost; fireRateMultiplier = fMultiplier; damageUpgradeBonus = dBonus; healthUpgradeBonus = hBonus; coolingRateMultiplier = cMultiplier; upgradeSprites = sprites;
    }

    private void Update()
    {
        HandleCooling();

        if (TargetingManager.Instance != null)
        {
            currentTarget = TargetingManager.Instance.GetAssignedTarget(this, 0f);
        }

        fireCooldown -= Time.unscaledDeltaTime;
        
        if (currentTarget != null && fireCooldown <= 0f && !isOverheated)
        {
            if (Time.timeScale > 0f)
            {
                if (isCryo) FireCryoPulse();
                else ShootBullet();
                
                fireCooldown = 1f / fireRate;
            }
        }

        UpdateHeatAndHPUI();
        HandleSelectionInput();
    }

    private void FireCryoPulse()
    {
        Debug.DrawLine(transform.position, currentTarget.position, new Color(0f, 0.902f, 1f, 1f), 0.2f);

        float speedMult = 0.5f; 
        float duration = 3.0f;
        
        if (currentUpgradeLevel == 1) { speedMult = 0.25f; duration = 3.5f; } 
        if (currentUpgradeLevel == 2) { speedMult = 0.0f; duration = 4.0f; }  

        EnemyContext enemyCtx = currentTarget.GetComponent<EnemyContext>();
        if (enemyCtx != null) { enemyCtx.TakeDamage(damage); enemyCtx.ApplySlow(speedMult, duration); }
        else
        {
            EnemyController oldCtrl = currentTarget.GetComponent<EnemyController>();
            if (oldCtrl != null) { oldCtrl.TakeDamage(damage); oldCtrl.ApplySlow(speedMult, duration); }

            LaserDroneController drone = currentTarget.GetComponent<LaserDroneController>();
            if (drone != null) { drone.TakeDamage(damage); drone.ApplySlow(speedMult, duration); }
        }

        currentHeat += heatPerShot;
        if (currentHeat >= maxHeat) isOverheated = true;
    }

    private void ShootBullet()
    {
        if (TargetingManager.Instance != null) TargetingManager.Instance.RegisterBulletFired(currentTarget, damage);

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
        if (currentHeat >= maxHeat) isOverheated = true;
    }

    public void ExecuteUpgrade()
    {
        if (currentUpgradeLevel >= maxUpgradeLevel) return;

        if (PlayerStats.Instance != null && PlayerStats.Instance.SpendCredits(upgradeCost))
        {
            currentUpgradeLevel++;
            damage += damageUpgradeBonus; 
            fireRate *= fireRateMultiplier;
            coolingRate *= coolingRateMultiplier;

            if (isCryo)
            {
                maxHealth = Mathf.RoundToInt(maxHealth * 1.25f);
                currentHealth = maxHealth;
            }
            else
            {
                maxHealth += healthUpgradeBonus;
                currentHealth += healthUpgradeBonus;
            }

            UpdateTurretVisuals();
        }
    }

    public void TakeDamage(int incomingDamage)
    {
        currentHealth -= incomingDamage;
        if (currentHealth < 0) currentHealth = 0;
        if (currentHealth <= 0) SellTurret(); 
    }

    private void HandleSelectionInput()
    {
        bool isClicked = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);
        if (isClicked && Time.timeScale >= 0f)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition);
            mouseWorldPos.z = 0f;

            if (Vector3.Distance(transform.position, mouseWorldPos) <= 0.5f && TurretUIManager.Instance != null)
            {
                TurretUIManager.Instance.OpenMenu(this);
            }
        }
    }

    public void SellTurret()
    {
        int totalRefund = 75 + (currentUpgradeLevel * 50);
        if (PlayerStats.Instance != null) PlayerStats.Instance.AddCredits(totalRefund);
        Destroy(gameObject);
    }

    private void HandleCooling()
    {
        if (currentHeat > 0f)
        {
            currentHeat -= coolingRate * Time.unscaledDeltaTime;
            if (currentHeat <= 0f) { currentHeat = 0f; isOverheated = false; }
        }
    }

    private void UpdateHeatAndHPUI()
    {
        if (fillBarImage != null)
        {
            fillBarImage.fillAmount = currentHeat / maxHeat;
            fillBarImage.color = isOverheated ? new Color(1f, 0.102f, 0.302f, 1f) : new Color(0f, 0.902f, 1f, 1f); 
        }
        if (turretHpText != null) turretHpText.text = $"{currentHealth} / {maxHealth} HP";
    }

    private void UpdateTurretVisuals()
    {
        if (spriteRenderer != null && upgradeSprites != null && currentUpgradeLevel < upgradeSprites.Length)
        {
            if (upgradeSprites[currentUpgradeLevel] != null) spriteRenderer.sprite = upgradeSprites[currentUpgradeLevel];
        }
    }

    public bool IsCryo() => isCryo;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    public class RuntimeTurretProxy { public int damage; public float fireRate; public float attackRange; public int currentUpgradeLevel; public int maxUpgradeLevel; public int upgradeCost; public float fireRateMultiplier; public int damageUpgradeBonus; public int healthUpgradeBonus; }
    public RuntimeTurretProxy GetTurretData() => new RuntimeTurretProxy { damage = this.damage, fireRate = this.fireRate, attackRange = this.attackRange, currentUpgradeLevel = this.currentUpgradeLevel, maxUpgradeLevel = this.maxUpgradeLevel, upgradeCost = this.upgradeCost, fireRateMultiplier = this.fireRateMultiplier, damageUpgradeBonus = this.damageUpgradeBonus, healthUpgradeBonus = this.healthUpgradeBonus };
}