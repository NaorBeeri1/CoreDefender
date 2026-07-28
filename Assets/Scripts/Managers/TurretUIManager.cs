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

        // Pause gameplay
        Time.timeScale = 0f;

        UpdateMenuDisplay();
    }

    public void UpdateMenuDisplay()
    {
        if (selectedTurret == null) return;

        TurretData data = selectedTurret.GetTurretData();
        if (data == null) return;

        if (data.currentUpgradeLevel >= data.maxUpgradeLevel)
        {
            if (statsDisplayText != null)
            {
                statsDisplayText.text = $"MAX TIER REACHED (Tier {data.currentUpgradeLevel}/{data.maxUpgradeLevel})\n\n" +
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

            if (statsDisplayText != null)
            {
                statsDisplayText.text = $"Tactic Level: Tier {data.currentUpgradeLevel} / {data.maxUpgradeLevel}\n\n" +
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

        // Resume normal game speed
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