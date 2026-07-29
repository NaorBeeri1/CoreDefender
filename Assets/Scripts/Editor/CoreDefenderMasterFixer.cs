using UnityEngine;
using UnityEditor;
using System.IO;

public class CoreDefenderMasterFixer : MonoBehaviour
{
    [MenuItem("Tools/CoreDefender/Apply Master Architecture & Range Fix")]
    public static void ApplyMasterFix()
    {
        // 1. Write ProjectileController.cs (Fixes bullets fading early and tracking properly)
        string projectileCode = @"
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header(""Projectile Stats"")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private int damage = 50;

    private Transform target;
    private Vector3 lastKnownTargetPos;
    private bool hasTargetPosition = false;

    public void SetTarget(Transform enemyTarget)
    {
        target = enemyTarget;
        if (target != null)
        {
            lastKnownTargetPos = target.position;
            hasTargetPosition = true;
        }
    }

    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }

    private void Update()
    {
        if (target != null)
        {
            lastKnownTargetPos = target.position;
        }
        else if (!hasTargetPosition)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 direction = (lastKnownTargetPos - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (target != null && Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            HitTarget();
            return;
        }

        if (target == null && Vector3.Distance(transform.position, lastKnownTargetPos) < 0.3f)
        {
            gameObject.SetActive(false);
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            TurretController turret = target.GetComponent<TurretController>();
            if (turret != null)
            {
                turret.TakeDamage(damage);
            }
            else
            {
                LaserDroneController drone = target.GetComponent<LaserDroneController>();
                if (drone != null)
                {
                    drone.TakeDamage(damage);
                }
                else
                {
                    EnemyContext enemyContext = target.GetComponent<EnemyContext>();
                    if (enemyContext != null)
                    {
                        enemyContext.TakeDamage(damage);
                    }
                    else
                    {
                        EnemyController oldController = target.GetComponent<EnemyController>();
                        if (oldController != null)
                        {
                            oldController.TakeDamage(damage);
                        }
                    }
                }
            }
        }
        gameObject.SetActive(false);
    }
}
";
        Directory.CreateDirectory("Assets/Scripts/Projectiles");
        File.WriteAllText("Assets/Scripts/Projectiles/ProjectileController.cs", projectileCode);

        // 2. Write BuildManager.cs (Fixes turret double-stacking / overlapping placement)
        string buildManagerCode = @"
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header(""References"")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject buildIndicatorPrefab; 
    [SerializeField] private GameObject turretPrefab; 

    [Header(""Grid Bounds"")]
    [SerializeField] private int gridWidth = 16;
    [SerializeField] private int gridHeight = 9;

    [Header(""Economic Cost"")]
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
            activeIndicator.name = ""BuildIndicator"";
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
            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            GameObject[] existingTurrets = GameObject.FindGameObjectsWithTag(""Turret"");
            foreach (GameObject t in existingTurrets)
            {
                if (Vector3.Distance(t.transform.position, mouseWorldPos) <= 0.5f)
                {
                    return; 
                }
            }

            float halfWidth = gridWidth / 2f;
            float halfHeight = gridHeight / 2f;

            if (currentGridPos.x >= 0 && currentGridPos.x < gridWidth && currentGridPos.y >= 0 && currentGridPos.y < gridHeight)
            {
                if (turretPrefab != null)
                {
                    if (PlayerStats.Instance != null && PlayerStats.Instance.SpendCredits(turretCost))
                    {
                        float spawnX = currentGridPos.x - halfWidth + 0.5f;
                        float spawnY = currentGridPos.y - halfHeight + 0.5f;
                        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

                        // Strict check to ensure no stacking on the exact coordinates
                        foreach (GameObject t in existingTurrets)
                        {
                            if (t != null && Vector3.Distance(t.transform.position, spawnPos) < 0.2f)
                            {
                                PlayerStats.Instance.AddCredits(turretCost); // Refund
                                return;
                            }
                        }

                        GameObject newTurret = Instantiate(turretPrefab, spawnPos, Quaternion.identity);
                        newTurret.tag = ""Turret"";
                    }
                }
            }
        }
    }
}
";
        Directory.CreateDirectory("Assets/Scripts/Managers");
        File.WriteAllText("Assets/Scripts/Managers/BuildManager.cs", buildManagerCode);

        // 3. Write TargetingManager.cs (True smart load-balancing distribution)
        string targetingManagerCode = @"
using UnityEngine;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header(""Dispatch Settings"")]
    [SerializeField] private float updateInterval = 0.2f;
    private float updateTimer = 0f;

    private Dictionary<TurretController, Transform> turretAssignments = new Dictionary<TurretController, Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            CleanAssignments();
            updateTimer = updateInterval;
        }
    }

    public Transform GetAssignedTarget(TurretController turret, float attackRange)
    {
        if (turretAssignments.TryGetValue(turret, out Transform currentTarget) && currentTarget != null)
        {
            float dist = Vector3.Distance(turret.transform.position, currentTarget.position);
            if (dist <= attackRange)
            {
                return currentTarget;
            }
        }

        Transform newTarget = CalculateSmartTarget(turret.transform, attackRange);
        turretAssignments[turret] = newTarget;
        return newTarget;
    }

    private Transform CalculateSmartTarget(Transform turretTransform, float attackRange)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(""Enemy"");
        if (enemies.Length == 0) return null;

        Dictionary<Transform, int> targetCounts = new Dictionary<Transform, int>();
        foreach (var kvp in turretAssignments)
        {
            if (kvp.Value != null)
            {
                if (!targetCounts.ContainsKey(kvp.Value)) targetCounts[kvp.Value] = 0;
                targetCounts[kvp.Value]++;
            }
        }

        Transform bestEnemy = null;
        float bestScore = float.MinValue;

        foreach (GameObject enemyObj in enemies)
        {
            if (enemyObj == null) continue;
            Transform enemy = enemyObj.transform;

            float dist = Vector3.Distance(turretTransform.position, enemy.position);
            if (dist > attackRange) continue; 

            float coreProximityScore = -enemy.position.x; 
            int currentAssignedTurrets = targetCounts.ContainsKey(enemy) ? targetCounts[enemy] : 0;
            
            float totalScore = coreProximityScore - (currentAssignedTurrets * 5f);

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestEnemy = enemy;
            }
        }

        if (bestEnemy == null)
        {
            float shortestDist = float.MaxValue;
            foreach (GameObject enemyObj in enemies)
            {
                if (enemyObj == null) continue;
                float dist = Vector3.Distance(turretTransform.position, enemyObj.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    bestEnemy = enemyObj.transform;
                }
            }
        }

        return bestEnemy;
    }

    private void CleanAssignments()
    {
        List<TurretController> keys = new List<TurretController>(turretAssignments.Keys);
        foreach (var turret in keys)
        {
            if (turret == null || turretAssignments[turret] == null)
            {
                turretAssignments.Remove(turret);
            }
        }
    }
}
";
        File.WriteAllText("Assets/Scripts/Managers/TargetingManager.cs", targetingManagerCode);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("CoreDefender", "Master architecture patch applied successfully! Double-stacking prevented, bullet fading eliminated, and smart distribution enabled.", "OK");
        Debug.Log("[CoreDefender] Master architecture patch successfully applied.");
    }
}