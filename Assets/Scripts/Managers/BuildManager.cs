using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject buildIndicatorPrefab; 

    [Header("Grid Bounds")]
    [SerializeField] private int gridWidth = 16;
    [SerializeField] private int gridHeight = 9;

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

        float worldX = snappedX - halfWidth + 0.5f;
        
        bool isWithinBounds = snappedX >= 0 && snappedX < gridWidth && snappedY >= 0 && snappedY < gridHeight;
        bool isSafeDistanceFromCore = worldX > -5.5f; 

        bool canPlaceHere = isWithinBounds && isSafeDistanceFromCore;

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
            if (ShopManager.Instance == null || !ShopManager.Instance.IsBuyingMode())
            {
                return;
            }

            float halfWidth = gridWidth / 2f;
            float halfHeight = gridHeight / 2f;

            if (currentGridPos.x < 0 || currentGridPos.x >= gridWidth || currentGridPos.y < 0 || currentGridPos.y >= gridHeight)
            {
                return; 
            }

            float spawnX = currentGridPos.x - halfWidth + 0.5f;
            float spawnY = currentGridPos.y - halfHeight + 0.5f;

            if (spawnX <= -5.5f)
            {
                return;
            }

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject activePrefab = ShopManager.Instance.GetActiveItemPrefab();
            int itemCost = ShopManager.Instance.GetActiveItemCost();

            if (activePrefab == null) return;

            GameObject[] existingStructures = GameObject.FindGameObjectsWithTag("Turret");
            foreach (GameObject t in existingStructures)
            {
                if (t != null && Vector3.Distance(t.transform.position, spawnPos) <= 0.2f)
                {
                    return; 
                }
            }

            ShopManager.Instance.ConsumePurchase(itemCost);

            GameObject newItem = Instantiate(activePrefab, spawnPos, Quaternion.identity);
            newItem.tag = "Turret"; 
        }
    }
}