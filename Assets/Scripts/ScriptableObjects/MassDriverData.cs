using UnityEngine;

[CreateAssetMenu(fileName = "NewMassDriverData", menuName = "CoreDefender/Mass Driver Data")]
public class MassDriverData : ScriptableObject
{
    [Header("Turret Identity")]
    public string turretName = "Mass Driver";
    public int cost = 350; // Premium late-game price

    [Header("Combat Stats")]
    public float fireRate = 0.3f;       // Very slow
    public float attackRange = 20f;     // Massive range
    public int damage = 250;            // Massive single-target burst

    [Header("Thermal Limits & Cooling")]
    public float maxHeat = 100f;
    public float heatPerShot = 50f;     // Overheats in 2 shots
    public float coolingRate = 20f;     // Slow to cool down

    [Header("Multi-Tier Upgrades")]
    public int currentUpgradeLevel = 0;
    public int maxUpgradeLevel = 2;
    public int upgradeCost = 300;
    public float fireRateMultiplier = 1.3f;
    public int damageUpgradeBonus = 150;
    public int healthUpgradeBonus = 75;
    public float coolingRateMultiplier = 1.5f;

    [Header("Visual Evolution")]
    public Sprite[] upgradeSprites;
}