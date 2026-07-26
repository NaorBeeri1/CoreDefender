using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Economy Settings")]
    [SerializeField] private int startingCredits = 300;
    private int currentCredits;

    public static PlayerStats Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentCredits = startingCredits;
        Debug.Log($"[CoreDefender] Player online. Starting Credits: {currentCredits}");
    }

    public int GetCurrentCredits()
    {
        return currentCredits;
    }

    public void AddCredits(int amount)
    {
        currentCredits += amount;
        Debug.Log($"[CoreDefender] Earned {amount} credits. Total: {currentCredits}");
    }

    public bool SpendCredits(int amount)
    {
        if (currentCredits >= amount)
        {
            currentCredits -= amount;
            Debug.Log($"[CoreDefender] Spent {amount} credits. Remaining: {currentCredits}");
            return true;
        }

        Debug.LogWarning("[CoreDefender] Insufficient energy credits!");
        return false;
    }
}