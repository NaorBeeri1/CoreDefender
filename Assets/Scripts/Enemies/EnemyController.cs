using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damageToCore = 10;

    [Header("Economy Reward")]
    [SerializeField] private int creditReward = 25;

    [Header("UI References")]
    [SerializeField] private GameObject enemyCanvasPrefab; // Drag EnemyCanvas prefab here
    private GameObject activeCanvasInstance;
    private Image healthFillImage;

    private int currentHealth;
    private Transform targetCore;
    private CoreManager coreManager;

    private void Start()
    {
        currentHealth = maxHealth;

        // Instantiate the floating health bar tightly above the enemy head (offset Y = 0.65f)
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

        // Keep world space canvas rotation locked/stable if parent moves
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
        Debug.Log("[CoreDefender] Enemy destroyed and bounty collected!");
        Destroy(gameObject);
    }
}