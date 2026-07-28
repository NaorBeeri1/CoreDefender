using System;
using UnityEngine;

public static class GameEventBus
{
    // Event triggered when any enemy is destroyed, passing the reward credit bounty
    public static event Action<int> OnEnemyDestroyed;

    public static void TriggerEnemyDestroyed(int creditReward)
    {
        OnEnemyDestroyed?.Invoke(creditReward);
    }
}