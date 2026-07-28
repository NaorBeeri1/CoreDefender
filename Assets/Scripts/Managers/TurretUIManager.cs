using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurretUIManager : MonoBehaviour
{
    public static TurretUIManager Instance { get; private set; }

    [Header("UI Panel References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
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

        // Pause the game like a true tactical pause screen
        Time.timeScale = 0f;

        if (selectedTurret.IsUpgraded())
        {
            if (upgradeButtonText != null) upgradeButtonText.text = "MAXED OUT";
            if (upgradeButton != null) upgradeButton.interactable = false;
        }
        else
        {
            if (upgradeButtonText != null) upgradeButtonText.text = $"UPGRADE ({selectedTurret.GetUpgradeCost()}c)";
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
            CloseMenu();
        }
    }
}