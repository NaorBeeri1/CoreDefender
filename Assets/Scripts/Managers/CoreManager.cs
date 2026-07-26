using UnityEngine;

public class CoreManager : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private int maxCoreHealth = 100;
    [SerializeField] private UIManager uiManager;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxCoreHealth;
        Debug.Log($"[CoreDefender] Core online. Current Health: {currentHealth}/{maxCoreHealth}");
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.LogWarning($"[CoreDefender] Core breached! Damage taken: {damageAmount}. Remaining Health: {currentHealth}/{maxCoreHealth}");

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.LogError("[CoreDefender] GAME OVER! The Core has been destroyed.");
        if (uiManager != null)
        {
            uiManager.ShowGameOver();
        }
    }
}