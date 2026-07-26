using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 50;
    [SerializeField] private int damageToCore = 10;

    [Header("Economy Reward")]
    [SerializeField] private int creditReward = 25;

    private Transform targetCore;
    private CoreManager coreManager;

    private void Start()
    {
        GameObject core = GameObject.FindWithTag("Core");
        if (core != null)
        {
            targetCore = core.transform;
            coreManager = core.GetComponent<CoreManager>();
        }
    }

    private void Update()
    {
        MoveTowardsCore();
    }

    private void MoveTowardsCore()
    {
        Vector3 destination = targetCore != null ? targetCore.position : Vector3.zero;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        // If reached the center, deal damage to core and destroy self
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            if (coreManager != null)
            {
                coreManager.TakeDamage(damageToCore);
            }
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCredits(creditReward);
        }
        Debug.Log("[CoreDefender] Enemy destroyed and bounty collected!");
        Destroy(gameObject);
    }
}