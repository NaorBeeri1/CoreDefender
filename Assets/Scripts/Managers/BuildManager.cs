using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject buildIndicatorPrefab; 
    [SerializeField] private GameObject turretPrefab; // Drag your TurretBase prefab here

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
        // Stop building interaction if the game is paused (e.g. Game Over)
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

        int snappedX = Mathf.FloorToInt(mouseWorldPos.x);
        int snappedY = Mathf.FloorToInt(mouseWorldPos.y);

        currentGridPos = new Vector2Int(snappedX, snappedY);

        // Check if cursor is within grid bounds
        bool isWithinBounds = snappedX >= 0 && snappedX < gridWidth && snappedY >= 0 && snappedY < gridHeight;

        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isWithinBounds);
            if (isWithinBounds)
            {
                activeIndicator.transform.position = new Vector3(snappedX + 0.5f, snappedY + 0.5f, 0f);
            }
        }
    }

    private void HandleGridInput()
    {
        bool isClicked = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);

        if (isClicked)
        {
            // Validate bounds before placing
            if (currentGridPos.x >= 0 && currentGridPos.x < gridWidth && currentGridPos.y >= 0 && currentGridPos.y < gridHeight)
            {
                if (turretPrefab != null)
                {
                    // Check if player has enough credits before building
                    if (PlayerStats.Instance != null && PlayerStats.Instance.SpendCredits(turretCost))
                    {
                        Vector3 spawnPos = new Vector3(currentGridPos.x + 0.5f, currentGridPos.y + 0.5f, 0f);
                        Instantiate(turretPrefab, spawnPos, Quaternion.identity);
                        Debug.Log($"Core Defender - Placed turret at: X = {currentGridPos.x}, Y = {currentGridPos.y}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Cannot build outside grid bounds!");
            }
        }
    }
}