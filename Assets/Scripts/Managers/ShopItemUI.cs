using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(2, 4)]
    [SerializeField] private string itemDescription = "Standard defensive structure.";

    private static TextMeshProUGUI sharedTooltipText;

    private void Start()
    {
        if (sharedTooltipText == null)
        {
            GameObject tooltipObj = GameObject.Find("ShopTooltipText");
            if (tooltipObj != null)
            {
                sharedTooltipText = tooltipObj.GetComponent<TextMeshProUGUI>();
                sharedTooltipText.text = "";
                sharedTooltipText.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sharedTooltipText != null)
        {
            sharedTooltipText.text = itemDescription;
            sharedTooltipText.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (sharedTooltipText != null)
        {
            sharedTooltipText.text = "";
            sharedTooltipText.gameObject.SetActive(false);
        }
    }
}