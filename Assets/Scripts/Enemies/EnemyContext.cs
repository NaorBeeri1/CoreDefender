using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyContext : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float baseMoveSpeed = 4.5f;
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
    private IEnemyState currentState;

    private float currentMoveSpeed;
    private bool isFrozen = false;
    private Coroutine slowCoroutine;

    public float MoveSpeed => currentMoveSpeed;
    public Transform TargetCore => targetCore;

    private void Start()
    {
        currentHealth = maxHealth;
        currentMoveSpeed = baseMoveSpeed;

        if (enemyCanvasPrefab != null)
        {
            activeCanvasInstance = Instantiate(enemyCanvasPrefab, transform.position + new Vector3(0f, 0.65f, 0f), Quaternion.identity, transform);
            Transform fillTrans = activeCanvasInstance.transform.Find("BackgroundBar/FillBar");
            if (fillTrans != null) healthFillImage = fillTrans.GetComponent<Image>();
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
        if (currentState != null) currentState.UpdateState(this);

        if (activeCanvasInstance != null)
        {
            activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);
        }
    }

    public void TransitionToState(IEnemyState nextState)
    {
        if (currentState != null) currentState.ExitState(this);
        currentState = nextState;
        currentState.EnterState(this);
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

    public void DealDamageToCore()
    {
        if (coreManager != null) coreManager.TakeDamage(damageToCore);
    }

    private void Die()
    {
        GameEventBus.TriggerEnemyDestroyed(creditReward);
        WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
        if (waveManager != null) waveManager.NotifyEnemyDefeated();
        Destroy(gameObject);
    }
}