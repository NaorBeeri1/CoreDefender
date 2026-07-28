using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damageToCore = 10;
    [SerializeField] private int creditReward = 25;

    [Header("UI References")]
    [SerializeField] private GameObject enemyCanvasPrefab; 
    private GameObject activeCanvasInstance;
    private Image healthFillImage;

    private int currentHealth;
    private Transform targetCore;
    private CoreManager coreManager;
    private float lastDamageTime = -1f;

    private void Start()
    {
        maxHealth = 100;
        currentHealth = maxHealth;

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
            targetCore = core.transform;
            coreManager = core.GetComponent<CoreManager>();
        }
    }

    private void Update()
    {
        MoveTowardsCore();
    }

    private void MoveTowardsCore()
    {
        Vector3 destination = targetCore != null ? targetCore.position : Vector3.zero;
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
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        // 0.15s gate allows rapid machine-gun fire while preventing frame-perfect stacking bugs
        if (Time.time - lastDamageTime < 0.15f) return;
        lastDamageTime = Time.time;

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
        Destroy(gameObject);
    }
}