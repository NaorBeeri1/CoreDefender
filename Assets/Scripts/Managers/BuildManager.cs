using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject buildIndicatorPrefab; 

    private Camera mainCam;
    private GameObject activeIndicator;
    private Vector2Int currentGridPos;

    private void Start()
    {
        mainCam = Camera.main;

        if (gridManager == null)
        {
            gridManager = Object.FindAnyObjectByType<GridManager>();
        }

        if (buildIndicatorPrefab != null)
        {
            activeIndicator = Instantiate(buildIndicatorPrefab);
            activeIndicator.name = "BuildIndicator";
            activeIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (activeIndicator != null)
            {
                activeIndicator.SetActive(false);
            }
            return;
        }

        UpdateCursorPosition();
        HandleGridInput();
    }

    private void UpdateCursorPosition()
    {
        if (mainCam == null || gridManager == null) return;

        Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Convert world position into exact grid cell coordinates using GridManager
        currentGridPos = gridManager.WorldToGrid(mouseWorldPos);

        // Access private width/height via public bounds check or re-derive
        // Assuming default GridManager width=9, height=6 based on your layout
        bool isWithinBounds = currentGridPos.x >= 0 && currentGridPos.x < 9 && 
                              currentGridPos.y >= 0 && currentGridPos.y < 6;

        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isWithinBounds);
            if (isWithinBounds)
            {
                // Snap indicator precisely to the center of the valid grid cell
                Vector3 cellCenterWorldPos = gridManager.GridToWorld(currentGridPos);
                activeIndicator.transform.position = cellCenterWorldPos;
            }
        }
    }

    private void HandleGridInput()
    {
        bool isClicked = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);

        if (isClicked)
        {
            if (ShopManager.Instance == null || !ShopManager.Instance.IsBuyingMode())
            {
                return;
            }

            if (currentGridPos.x < 0 || currentGridPos.x >= 9 || currentGridPos.y < 0 || currentGridPos.y >= 6)
            {
                return; // Outside the 6x9 grid bounds
            }

            Vector3 spawnPos = gridManager.GridToWorld(currentGridPos);

            GameObject[] existingStructures = GameObject.FindGameObjectsWithTag("Turret");
            foreach (GameObject t in existingStructures)
            {
                if (t != null && Vector3.Distance(t.transform.position, spawnPos) <= 0.2f)
                {
                    return; // Cell already occupied
                }
            }

            GameObject activePrefab = ShopManager.Instance.GetActiveItemPrefab();
            int itemCost = ShopManager.Instance.GetActiveItemCost();

            if (activePrefab == null) return;

            ShopManager.Instance.ConsumePurchase(itemCost);

            GameObject newItem = Instantiate(activePrefab, spawnPos, Quaternion.identity);
            newItem.tag = "Turret"; 
        }
    }
}