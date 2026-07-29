using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Configuration")]
    [SerializeField] private GameObject turretPrefab; 
    [SerializeField] private int baseTurretCost = 100;

    [Header("UI References")]
    [SerializeField] private Button turretButton1; 
    [SerializeField] private TextMeshProUGUI currentBuyingText; 

    private bool isBuyingMode = false;
    private string selectedTurretName = "None";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (turretButton1 != null)
        {
            turretButton1.onClick.AddListener(() => SelectTurretToBuy("Base Turret", baseTurretCost));
        }
        UpdateBuyingText();
    }

    private void Update()
    {
        // Auto-cancel buying mode if player runs out of money while placing
        if (isBuyingMode && PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() < baseTurretCost)
        {
            CancelPurchase();
        }
    }

    public void SelectTurretToBuy(string turretName, int cost)
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() >= cost)
        {
            isBuyingMode = true;
            selectedTurretName = turretName;
            Debug.Log($"[ShopManager] Continuous buying mode active for: {selectedTurretName}");
        }
        else
        {
            Debug.LogWarning("[ShopManager] Not enough credits!");
            CancelPurchase();
        }
        UpdateBuyingText();
    }

    public bool IsBuyingMode()
    {
        return isBuyingMode;
    }

    public void ConsumePurchase(int cost)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SpendCredits(cost);
        }
        
        // DO NOT set isBuyingMode to false here! 
        // This keeps continuous building active until money runs out or player cancels.
        
        if (PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() < baseTurretCost)
        {
            CancelPurchase();
        }
    }

    public void CancelPurchase()
    {
        isBuyingMode = false;
        selectedTurretName = "None";
        UpdateBuyingText();
    }

    private void UpdateBuyingText()
    {
        if (currentBuyingText != null)
        {
            if (isBuyingMode)
            {
                currentBuyingText.text = $"Currently Buying: <color=#1AE6FF>{selectedTurretName}</color>";
            }
            else
            {
                currentBuyingText.text = "Currently Buying: None";
            }
        }
    }
}