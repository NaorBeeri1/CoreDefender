using UnityEngine;

public class CorePerkManager : MonoBehaviour
{
    public static CorePerkManager Instance { get; private set; }

    [Header("Active Global Perk")]
    [SerializeField] private CorePerkData activePerk;

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
        ApplyPerkBonuses();
    }

    private void ApplyPerkBonuses()
    {
        if (activePerk == null) return;

        Debug.Log($"[CoreDefender] Applying Core Perk: {activePerk.perkName}");

        CoreManager core = Object.FindAnyObjectByType<CoreManager>();
        if (core != null)
        {
            Debug.Log($"[CoreDefender] Core upgraded with +{activePerk.coreHealthBonus} integrity!");
        }
    }

    public float GetFireRateMultiplier()
    {
        return activePerk != null ? activePerk.fireRateBonusMultiplier : 1f;
    }

    public float GetCreditMultiplier()
    {
        return activePerk != null ? activePerk.creditBonusMultiplier : 1f;
    }
}