using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurretUIManager : MonoBehaviour
{
    public static TurretUIManager Instance { get; private set; }

    [Header("UI Panel References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI statsDisplayText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellButtonText; 
    [SerializeField] private Button closeButton;

    private TurretController selectedTurret;

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
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellClicked);
            if (sellButtonText == null)
            {
                sellButtonText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }
    }

    public void OpenMenu(TurretController turret)
    {
        selectedTurret = turret;
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        UpdateMenuDisplay();
    }

    public void UpdateMenuDisplay()
    {
        if (selectedTurret == null) return;

        TurretData data = selectedTurret.GetTurretData();
        if (data == null) return;

        int baseRefund = 75;
        int upgradeRefundBonus = 50;
        int totalRefund = baseRefund + (data.currentUpgradeLevel * upgradeRefundBonus);

        if (sellButtonText != null)
        {
            sellButtonText.text = $"SELL (Refund {totalRefund}c)";
        }

        int currentHP = selectedTurret.GetCurrentHealth();
        int maxHP = selectedTurret.GetMaxHealth();

        if (data.currentUpgradeLevel >= data.maxUpgradeLevel)
        {
            if (statsDisplayText != null)
            {
                statsDisplayText.text = $"MAX TIER REACHED (Tier {data.currentUpgradeLevel}/{data.maxUpgradeLevel})\n\n" +
                                        $"Current Health: {currentHP} / {maxHP} HP\n" +
                                        $"Current Damage: {data.damage} HP\n" +
                                        $"Current Fire Rate: {data.fireRate:F1} /s\n" +
                                        $"Attack Range: {data.attackRange}";
            }
            if (upgradeButtonText != null) upgradeButtonText.text = "MAXED OUT";
            if (upgradeButton != null) upgradeButton.interactable = false;
        }
        else
        {
            int nextDamage = data.damage + data.damageUpgradeBonus;
            float nextFireRate = data.fireRate * data.fireRateMultiplier;
            int nextMaxHP = maxHP + data.healthUpgradeBonus;

            if (statsDisplayText != null)
            {
                statsDisplayText.text = $"Tactic Level: Tier {data.currentUpgradeLevel} / {data.maxUpgradeLevel}\n\n" +
                                        $"Health: {currentHP} / {maxHP} HP  ->  <color=#1AE6FF>{currentHP} / {nextMaxHP} HP</color>\n" +
                                        $"Damage: {data.damage} HP  ->  <color=#1AE6FF>{nextDamage} HP</color>\n" +
                                        $"Fire Rate: {data.fireRate:F1}/s  ->  <color=#1AE6FF>{nextFireRate:F1}/s</color>\n" +
                                        $"Range: {data.attackRange}";
            }
            if (upgradeButtonText != null) upgradeButtonText.text = $"UPGRADE ({data.upgradeCost}c)";
            if (upgradeButton != null) upgradeButton.interactable = true;
        }
    }

    public void CloseMenu()
    {
        selectedTurret = null;
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void OnUpgradeClicked()
    {
        if (selectedTurret != null)
        {
            selectedTurret.ExecuteUpgrade();
            UpdateMenuDisplay(); 
        }
    }

    private void OnSellClicked()
    {
        if (selectedTurret != null)
        {
            selectedTurret.SellTurret();
            CloseMenu();
        }
    }
}