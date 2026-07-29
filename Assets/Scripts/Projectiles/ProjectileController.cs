
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Stats")]
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
