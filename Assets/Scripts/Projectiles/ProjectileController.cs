using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 50;
    [SerializeField] private float maxTravelDistance = 50f; // Expanded board-wide infinite range

    private Transform target;
    private Vector3 startPosition;
    private Vector3 lastKnownTargetPos;
    private bool hasTargetPosition = false;

    private void OnEnable()
    {
        startPosition = transform.position;
        hasTargetPosition = false;
    }

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

        // Check impact with live target
        if (target != null && Vector3.Distance(transform.position, target.position) < 0.4f)
        {
            HitTarget();
            return;
        }

        // Check fallback destination reach if target was destroyed mid-air
        if (target == null && Vector3.Distance(transform.position, lastKnownTargetPos) < 0.4f)
        {
            gameObject.SetActive(false);
            return;
        }

        // Failsafe boundary check for infinite distance travel
        if (Vector3.Distance(startPosition, transform.position) > maxTravelDistance)
        {
            gameObject.SetActive(false);
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.NotifyBulletHit(target, damage);
            }

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