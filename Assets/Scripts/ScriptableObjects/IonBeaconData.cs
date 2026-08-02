using UnityEngine;

[CreateAssetMenu(fileName = "NewIonBeaconData", menuName = "CoreDefender/Ion Beacon Data")]
public class IonBeaconData : ScriptableObject
{
    [Header("Turret Identity")]
    public string turretName = "Ion Beacon";
    public int cost = 500; // Ultimate end-game asset

    [Header("Combat Stats")]
    public float fireRate = 0.2f;       
    public float attackRange = 999f;    // Infinite board coverage
    public int damage = 400;            // Eradicates standard units instantly

    [Header("Thermal Limits & Cooling")]
    public float maxHeat = 100f;
    public float heatPerShot = 100f;    // Instant overheat per strike (requires cycle management)
    public float coolingRate = 15f;

    [Header("Multi-Tier Upgrades")]
    public int currentUpgradeLevel = 0;
    public int maxUpgradeLevel = 1;     // 1 Supercharged tier
    public int upgradeCost = 500;
    public float fireRateMultiplier = 1.5f;
    public int damageUpgradeBonus = 300;
    public int healthUpgradeBonus = 100;
    public float coolingRateMultiplier = 2.0f;

    [Header("Visual Evolution")]
    public Sprite[] upgradeSprites;
}