using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LaserDroneController : MonoBehaviour
{
    [Header("Drone Combat Stats")]
    [SerializeField] private float baseMoveSpeed = 3f;
    [SerializeField] private int maxHealth = 150; 
    [SerializeField] private int damageToCore = 10;
    [SerializeField] private float fireRate = 0.5f; 
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

    private float currentMoveSpeed;
    private bool isFrozen = false;
    private Coroutine slowCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
        currentMoveSpeed = baseMoveSpeed;
        startYPos = transform.position.y;
        gameObject.tag = "Enemy";

        if (enemyCanvasPrefab != null)
        {
            activeCanvasInstance = Instantiate(enemyCanvasPrefab, transform.position + new Vector3(0f, 0.65f, 0f), Quaternion.identity, transform);
            Transform fillTrans = activeCanvasInstance.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null) healthFillImage = fillTrans.GetComponent<Image>();
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
            float targetY = startYPos + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(rightSideX, targetY, 0f), currentMoveSpeed * Time.deltaTime);

            if (activeCanvasInstance != null) activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);

            if (fireCooldown <= 0f && !isFrozen) // Cannot shoot back if frozen
            {
                GameObject randomTurret = turrets[Random.Range(0, turrets.Length)];
                ShootAtTurret(randomTurret.transform);
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            if (coreTarget != null)
            {
                Vector3 destination = coreTarget.position;
                transform.position = Vector3.MoveTowards(transform.position, destination, currentMoveSpeed * Time.deltaTime);

                if (activeCanvasInstance != null) activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);

                if (Vector3.Distance(transform.position, destination) < 0.1f)
                {
                    if (coreManager != null) coreManager.TakeDamage(damageToCore);
                    Die();
                }
            }
        }
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float slowMultiplier, float duration)
    {
        isFrozen = true;
        currentMoveSpeed = baseMoveSpeed * slowMultiplier;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0f, 0.902f, 1f, 1f); // #00E5FF Cyan

        yield return new WaitForSeconds(duration);

        currentMoveSpeed = baseMoveSpeed;
        if (sr != null) sr.color = Color.white; // #FFFFFF Normal
        isFrozen = false;
        slowCoroutine = null;
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
                proj.SetDamage(15); 
            }
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public bool IsFrozen() => isFrozen;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthUI();

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthUI()
    {
        if (healthFillImage != null) healthFillImage.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        GameEventBus.TriggerEnemyDestroyed(creditReward);
        WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
        if (waveManager != null) waveManager.NotifyEnemyDefeated();
        Destroy(gameObject);
    }
}