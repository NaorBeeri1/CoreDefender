using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretData", menuName = "CoreDefender/Turret Data")]
public class TurretData : ScriptableObject
{
    [Header("Turret Identity")]
    public string turretName = "Standard Turret";
    public int cost = 100;

    [Header("Base Combat Stats")]
    public float fireRate = 2.5f;     
    public float attackRange = 5f;
    public int damage = 50;            

    [Header("Thermal Limits & Cooling")]
    public float maxHeat = 100f;
    public float heatPerShot = 15f;
    public float coolingRate = 40f;   

    [Header("Multi-Tier Upgrades")]
    public int currentUpgradeLevel = 0;
    public int maxUpgradeLevel = 3;
    public int upgradeCost = 150;
    public float fireRateMultiplier = 1.2f;
    public int damageUpgradeBonus = 25; 
    public int healthUpgradeBonus = 25; 
    public float coolingRateMultiplier = 1.5f; 

    [Header("Visual Evolution")]
    public Sprite[] upgradeSprites; // Index 0 = Base Level, Index 1 = Tier 1, etc.
}