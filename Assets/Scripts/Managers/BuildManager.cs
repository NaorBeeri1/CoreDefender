using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject buildIndicatorPrefab; 
    [SerializeField] private GameObject turretPrefab; 

    [Header("Grid Bounds")]
    [SerializeField] private int gridWidth = 16;
    [SerializeField] private int gridHeight = 9;

    [Header("Economic Cost")]
    [SerializeField] private int turretCost = 100;

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

        bool isWithinBounds = snappedX >= 0 && snappedX < gridWidth && snappedY >= 0 && snappedY < gridHeight;

        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isWithinBounds);
            if (isWithinBounds)
            {
                float worldX = snappedX - halfWidth + 0.5f;
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

            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            float spawnX = currentGridPos.x - halfWidth + 0.5f;
            float spawnY = currentGridPos.y - halfHeight + 0.5f;
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject[] existingTurrets = GameObject.FindGameObjectsWithTag("Turret");
            foreach (GameObject t in existingTurrets)
            {
                if (t != null && Vector3.Distance(t.transform.position, spawnPos) <= 0.2f)
                {
                    return; // Spot already occupied
                }
            }

            // Deduct credits and keep building mode active!
            ShopManager.Instance.ConsumePurchase(100);

            GameObject newTurret = Instantiate(turretPrefab, spawnPos, Quaternion.identity);
            newTurret.tag = "Turret";
        }
    }
}