using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject buildIndicatorPrefab; 

    [Header("Grid Bounds & Offsets")]
    [SerializeField] private int gridWidth = 16;
    [SerializeField] private int gridHeight = 9;
    [SerializeField] private int minGridX = 2; // <-- Adjust this in the Inspector to block columns behind the core!

    private Camera mainCam;
    private GameObject activeIndicator;
    private Vector2Int currentGridPos;

    private void Start()
    {
        mainCam = Camera.main;

        if (buildIndicatorPrefab != null)
        {
            activeIndicator = Instantiate(buildIndicatorPrefab);
            activeIndicator.name = "BuildIndicator";
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
        if (mainCam == null) return;

        Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        float halfWidth = gridWidth / 2f;
        float halfHeight = gridHeight / 2f;

        int snappedX = Mathf.FloorToInt(mouseWorldPos.x + halfWidth);
        int snappedY = Mathf.FloorToInt(mouseWorldPos.y + halfHeight);

        currentGridPos = new Vector2Int(snappedX, snappedY);

<<<<<<< Updated upstream
        float worldX = snappedX - halfWidth + 0.5f;
        
        bool isWithinBounds = snappedX >= 0 && snappedX < gridWidth && snappedY >= 0 && snappedY < gridHeight;
        bool isSafeDistanceFromCore = worldX > -5.5f; 

        bool canPlaceHere = isWithinBounds && isSafeDistanceFromCore;
=======
        // Enforce that snappedX must be >= minGridX to prevent building behind the core
        bool isWithinBounds = snappedX >= minGridX && snappedX < gridWidth && snappedY >= 0 && snappedY < gridHeight;
>>>>>>> Stashed changes

        if (activeIndicator != null)
        {
            activeIndicator.SetActive(canPlaceHere);
            if (canPlaceHere)
            {
                float worldY = snappedY - halfHeight + 0.5f;
                activeIndicator.transform.position = new Vector3(worldX, worldY, 0f);
            }
        }
    }

    private void HandleGridInput()
    {
        bool isClicked = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);

        if (isClicked)
        {
            if (ShopManager.Instance == null)
            {
                Debug.LogError("[BuildManager DEBUG] ShopManager.Instance is NULL!");
                return;
            }

            if (!ShopManager.Instance.IsBuyingMode())
            {
                Debug.LogWarning("[BuildManager DEBUG] Click detected, but IsBuyingMode() is FALSE! Did you select an item from the shop?");
                return;
            }

            // Strict check: Block clicks outside the valid grid zone (including behind the core)
            if (currentGridPos.x < minGridX || currentGridPos.x >= gridWidth || currentGridPos.y < 0 || currentGridPos.y >= gridHeight)
            {
                Debug.LogWarning("[BuildManager DEBUG] Clicked outside grid bounds.");
                return; 
            }

<<<<<<< Updated upstream
=======
            float halfWidth = gridWidth / 2f;
            float halfHeight = gridHeight / 2f;

            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

>>>>>>> Stashed changes
            float spawnX = currentGridPos.x - halfWidth + 0.5f;
            float spawnY = currentGridPos.y - halfHeight + 0.5f;

            if (spawnX <= -5.5f)
            {
                Debug.LogWarning("[BuildManager DEBUG] Placement blocked: Too close to the Core.");
                return;
            }

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject activePrefab = ShopManager.Instance.GetActiveItemPrefab();
            int itemCost = ShopManager.Instance.GetActiveItemCost();

            if (activePrefab == null)
            {
                Debug.LogError("[BuildManager DEBUG] Active Item Prefab from ShopManager is NULL! Make sure the Bomb Prefab slot is filled on the ShopManager component.");
                return;
            }

            // Check overlap
            GameObject[] existingStructures = GameObject.FindGameObjectsWithTag("Turret");
            foreach (GameObject t in existingStructures)
            {
                if (t != null && Vector3.Distance(t.transform.position, spawnPos) <= 0.2f)
                {
                    Debug.LogWarning("[BuildManager DEBUG] Spot already occupied by an existing structure.");
                    return; 
                }
            }

<<<<<<< Updated upstream
            if (PlayerStats.Instance != null && PlayerStats.Instance.GetCurrentCredits() < itemCost)
            {
                Debug.LogWarning("[BuildManager DEBUG] Not enough credits to complete purchase.");
                return;
            }
=======
            ShopManager.Instance.ConsumePurchase(100);
>>>>>>> Stashed changes

            Debug.Log($"[BuildManager DEBUG] Successfully spawning {activePrefab.name} at position {spawnPos} costing {itemCost} credits.");

            ShopManager.Instance.ConsumePurchase(itemCost);

            GameObject newItem = Instantiate(activePrefab, spawnPos, Quaternion.identity);
            newItem.tag = "Turret"; 
        }
    }
}