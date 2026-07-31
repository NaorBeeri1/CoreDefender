using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float baseMoveSpeed = 3f;
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

    private float currentMoveSpeed;
    private bool isFrozen = false;
    private Coroutine slowCoroutine;

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
    }

    private void Update()
    {
        MoveTowardsCore();
    }

    private void MoveTowardsCore()
    {
        Vector3 destination = targetCore != null ? targetCore.position : Vector3.zero;
        transform.position = Vector3.MoveTowards(transform.position, destination, currentMoveSpeed * Time.deltaTime);

        if (activeCanvasInstance != null) activeCanvasInstance.transform.position = transform.position + new Vector3(0f, 0.65f, 0f);

        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            if (coreManager != null) coreManager.TakeDamage(damageToCore);
            Destroy(gameObject);
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

    public int GetCurrentHealth() => currentHealth;
    public bool IsFrozen() => isFrozen;

    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < 0.15f) return;
        lastDamageTime = Time.time;

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
        Destroy(gameObject);
    }
}