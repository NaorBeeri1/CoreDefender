using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Configuration")]
    [SerializeField] private GameObject turretPrefab; 
    [SerializeField] private GameObject sniperTurretPrefab; // Added Sniper Prefab
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int baseTurretCost = 100;
    [SerializeField] private int sniperTurretCost = 200;    // Added Sniper Cost
    [SerializeField] private int bombCost = 50;

    [Header("UI References")]
    [SerializeField] private Button turretButton1; 
    [SerializeField] private Button sniperButton;         // Added Sniper Button Reference
    [SerializeField] private Button bombButton; 
    [SerializeField] private TextMeshProUGUI currentBuyingText; 

    private bool isBuyingMode = false;
    private string selectedItemName = "None";
    private GameObject activeItemPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (turretButton1 != null)
        {
            turretButton1.onClick.AddListener(() => SelectItemToBuy("Base Turret", baseTurretCost, turretPrefab));
        }

        if (sniperButton != null)
        {
            sniperButton.onClick.AddListener(() => SelectItemToBuy("Laser Sniper", sniperTurretCost, sniperTurretPrefab));
        }

        if (bombButton != null)
        {
            bombButton.onClick.AddListener(() => SelectItemToBuy("Tactical Bomb", bombCost, bombPrefab));
        }
        UpdateBuyingText();
    }

    private void Update()
    {
        int currentCost = GetActiveItemCost();
        if (isBuyingMode && PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() < currentCost)
        {
            CancelPurchase();
        }
    }

    public void SelectItemToBuy(string itemName, int cost, GameObject itemPrefab)
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() >= cost)
        {
            isBuyingMode = true;
            selectedItemName = itemName;
            activeItemPrefab = itemPrefab;
            Debug.Log($"[ShopManager] Continuous buying mode active for: {selectedItemName}");
        }
        else
        {
            Debug.LogWarning("[ShopManager] Not enough credits!");
            CancelPurchase();
        }
        UpdateBuyingText();
    }

    public GameObject GetActiveItemPrefab()
    {
        return activeItemPrefab;
    }

    public int GetActiveItemCost()
    {
        if (selectedItemName == "Laser Sniper") return sniperTurretCost;
        if (selectedItemName == "Tactical Bomb") return bombCost;
        return baseTurretCost;
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

        int currentCost = GetActiveItemCost();
        if (PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() < currentCost)
        {
            CancelPurchase();
        }
    }

    public void CancelPurchase()
    {
        isBuyingMode = false;
        selectedItemName = "None";
        activeItemPrefab = null;
        UpdateBuyingText();
    }

    private void UpdateBuyingText()
    {
        if (currentBuyingText != null)
        {
            if (isBuyingMode)
            {
                currentBuyingText.text = $"Currently Buying: <color=#1AE6FF>{selectedItemName}</color>";
            }
            else
            {
                currentBuyingText.text = "Currently Buying: None";
            }
        }
    }
}