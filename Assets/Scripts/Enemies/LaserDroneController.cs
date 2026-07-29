using UnityEngine;
using UnityEngine.UI;

public class LaserDroneController : MonoBehaviour
{
    [Header("Drone Combat Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int maxHealth = 150; 
    [SerializeField] private int damageToCore = 10;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float fireRate = 0.5f; // Slower fire rate (once every 2 seconds)
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private int creditReward = 50;

    [Header("Floating Behavior")]
    [SerializeField] private float rightSideX = 7f;      
    [SerializeField] private float floatAmplitude = 2f;  
    [SerializeField] private float floatFrequency = 1.5f;

    [Header("UI References")]
    [SerializeField] private GameObject enemyCanvasPrefab; 
    private GameObject activeCanvasInstance;
    private Image healthFillImage;

    private int currentHealth;
    private float fireCooldown = 0f;
    private Transform coreTarget;
    private CoreManager coreManager;
    private float startYPos;
    bool turretsCleared = false;

    private void Start()
    {
        currentHealth = maxHealth;
        startYPos = transform.position.y;
        gameObject.tag = "Enemy";

        // Spawn health bar above drone
        if (enemyCanvasPrefab != null)
        {
            activeCanvasInstance = Instantiate(enemyCanvasPrefab, transform.position + new Vector3(0f, 0.65f, 0f), Quaternion.identity, transform);
            Transform fillTrans = activeCanvasInstance.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null)
            {
                healthFillImage = fillTrans.GetComponent<Image>();
            }
        }

        GameObject core = GameObject.FindWithTag("Core");
        if (core != null)
        {
            coreTarget = core.transform;
            coreManager = core.GetComponent<CoreManager>();
        }
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");

        if (turrets.Length > 0)
        {
            turretsCleared = false;

            // Hover up and down on the right side of the screen
            float targetY = startYPos + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(rightSideX, targetY, 0f), moveSpeed * Time.deltaTime);

            if (activeCanvasInstance != null)
            {
                activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);
            }

            if (fireCooldown <= 0f)
            {
                GameObject randomTurret = turrets[Random.Range(0, turrets.Length)];
                ShootAtTurret(randomTurret.transform);
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            turretsCleared = true;
            if (coreTarget != null)
            {
                Vector3 destination = coreTarget.position;
                transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

                if (activeCanvasInstance != null)
                {
                    activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);
                }

                if (Vector3.Distance(transform.position, destination) < 0.1f)
                {
                    if (coreManager != null)
                    {
                        coreManager.TakeDamage(damageToCore);
                    }
                    Die();
                }
            }
        }
    }

    private void ShootAtTurret(Transform target)
    {
        if (enemyBulletPrefab != null && target != null)
        {
            GameObject bulletObj = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            ProjectileController proj = bulletObj.GetComponent<ProjectileController>();
            if (proj != null)
            {
                proj.SetTarget(target);
                proj.SetDamage(15); // Balanced turret damage (takes ~7 hits to destroy a 100 HP turret)
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        GameEventBus.TriggerEnemyDestroyed(creditReward);

        WaveManager waveManager = Object.FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.NotifyEnemyDefeated();
        }

        Destroy(gameObject);
    }
}