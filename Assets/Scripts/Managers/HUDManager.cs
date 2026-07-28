using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI coreHealthText;

    [Header("Component References")]
    [SerializeField] private CoreManager coreManager;

    private void Update()
    {
        UpdateCreditsDisplay();
        UpdateCoreHealthDisplay();
    }

    private void UpdateCreditsDisplay()
    {
        if (creditsText != null && PlayerStats.Instance != null)
        {
            creditsText.text = $"Credits: {PlayerStats.Instance.GetCurrentCredits()}";
        }
    }

    private void UpdateCoreHealthDisplay()
    {
        if (coreHealthText != null && coreManager != null)
        {
            coreHealthText.text = $"Core HP: {coreManager.GetCurrentHealth()}/{coreManager.GetMaxHealth()}";
        }
    }
}