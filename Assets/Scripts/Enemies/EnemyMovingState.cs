using UnityEngine;

public class EnemyMovingState : IEnemyState
{
    public void EnterState(EnemyContext enemy)
    {
        // State initialization if needed
    }

    public void UpdateState(EnemyContext enemy)
    {
        Vector3 destination = enemy.TargetCore != null ? enemy.TargetCore.position : Vector3.zero;
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, destination, enemy.MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(enemy.transform.position, destination) < 0.1f)
        {
            enemy.DealDamageToCore();

            // Notify WaveManager before destroying so the wave counter advances correctly
            WaveManager waveManager = Object.FindObjectOfType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.NotifyEnemyDefeated();
            }

            Object.Destroy(enemy.gameObject);
        }
    }

    public void ExitState(EnemyContext enemy)
    {
        // Cleanup if needed
    }
}