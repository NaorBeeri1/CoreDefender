using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 50;

    private Transform target;

    public void SetTarget(Transform enemyTarget)
    {
        target = enemyTarget;
    }

    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }

    private void Update()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            // Check for EnemyContext (State Pattern Architecture)
            EnemyContext enemyContext = target.GetComponent<EnemyContext>();
            if (enemyContext != null)
            {
                enemyContext.TakeDamage(damage);
            }
            else
            {
                // Fallback check if old EnemyController is still present
                EnemyController oldController = target.GetComponent<EnemyController>();
                if (oldController != null)
                {
                    oldController.TakeDamage(damage);
                }
            }
        }
        gameObject.SetActive(false);
    }
}