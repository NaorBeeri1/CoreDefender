using UnityEngine;

[CreateAssetMenu(fileName = "NewSniperTurretData", menuName = "CoreDefender/Sniper Turret Data")]
public class SniperTurretData : ScriptableObject
{
    [Header("Turret Identity")]
    public string turretName = "Laser Sniper";
    public int cost = 200;

    [Header("Combat Stats")]
    public float fireRate = 0.6f;       // Slow firing
    public float attackRange = 15f;     // Long range across the board
    public int damage = 100;            // Base damage starting at 100

    [Header("Thermal Limits & Cooling")]
    public float maxHeat = 100f;
    public float heatPerShot = 35f;
    public float coolingRate = 25f;

    [Header("Multi-Tier Upgrades (2 Levels Total)")]
    public int currentUpgradeLevel = 0;
    public int maxUpgradeLevel = 1;     // Level 0 (Base) and Level 1 (Max)
    public int upgradeCost = 250;
    public float fireRateMultiplier = 1.5f;  // Multiplies fire rate by 1.5x on upgrade
    public int damageUpgradeBonus = 100;     // Upgrades damage from 100 to 200
    public int healthUpgradeBonus = 50;      // Adds 50% health bonus
    public float coolingRateMultiplier = 1.3f;

    [Header("Visual Evolution")]
    public Sprite[] upgradeSprites;     // Index 0 = Base Level, Index 1 = Max Level
}