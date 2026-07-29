using UnityEngine;
using UnityEngine.UI;

public class EnemyContext : MonoBehaviour
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

    private IEnemyState currentState;

    public float MoveSpeed => moveSpeed;
    public Transform TargetCore => targetCore;

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

        TransitionToState(new EnemyMovingState());
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }

        if (activeCanvasInstance != null)
        {
            activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);
        }
    }

    public void TransitionToState(IEnemyState nextState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = nextState;
        currentState.EnterState(this);
    }

    public void TakeDamage(int damage)
    {
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

    public void DealDamageToCore()
    {
        if (coreManager != null)
        {
            coreManager.TakeDamage(damageToCore);
        }
    }

    private void Die()
    {
        GameEventBus.TriggerEnemyDestroyed(creditReward);
        Destroy(gameObject);
    }
}