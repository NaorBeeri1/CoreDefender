using UnityEngine;

public class BombController : MonoBehaviour
{
    [Header("Data Profile")]
    [SerializeField] private BombData bombData;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        if (bombData == null)
        {
            bombData = ScriptableObject.CreateInstance<BombData>();
            bombData.damage = 100;
            bombData.detonationRange = 2f;
        }
    }

    private void Update()
    {
        CheckForEnemies();
    }

    private void CheckForEnemies()
    {
        if (bombData == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= bombData.detonationRange)
                {
                    Debug.Log($"[Bomb DEBUG] Enemy {enemy.name} within range {distance:F2}. Detonating!");
                    Detonate();
                    break;
                }
            }
        }
    }

    private void Detonate()
    {
        // Find all enemies in range directly using distance checks (bypasses missing 2D Colliders)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= bombData.detonationRange)
                {
                    EnemyContext enemyCtx = enemy.GetComponent<EnemyContext>();
                    if (enemyCtx != null)
                    {
                        Debug.Log($"[Bomb DEBUG] Dealing {bombData.damage} damage to EnemyContext on {enemy.name}");
                        enemyCtx.TakeDamage(bombData.damage);
                        continue;
                    }

                    EnemyController oldEnemy = enemy.GetComponent<EnemyController>();
                    if (oldEnemy != null)
                    {
                        Debug.Log($"[Bomb DEBUG] Dealing {bombData.damage} damage to EnemyController on {enemy.name}");
                        oldEnemy.TakeDamage(bombData.damage);
                        continue;
                    }

                    LaserDroneController drone = enemy.GetComponent<LaserDroneController>();
                    if (drone != null)
                    {
                        Debug.Log($"[Bomb DEBUG] Dealing {bombData.damage} damage to LaserDroneController on {enemy.name}");
                        drone.TakeDamage(bombData.damage);
                        continue;
                    }
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (bombData != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, bombData.detonationRange);
        }
    }
}