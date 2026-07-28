using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretData", menuName = "CoreDefender/Turret Data")]
public class TurretData : ScriptableObject
{
    [Header("Turret Identity")]
    public string turretName = "Standard Turret";
    public int cost = 100;

    [Header("Combat Stats")]
    public float fireRate = 1f;       // Shots per second
    public float attackRange = 5f;
    public int damage = 25;

    [Header("Thermal Limits")]
    public float maxHeat = 100f;
    public float heatPerShot = 15f;
    public float coolingRate = 25f;   // Heat lost per second when idle

    [Header("Upgrade Scaling")]
    public int upgradeCost = 150;
    public float fireRateMultiplier = 1.25f;
    public int damageUpgradeBonus = 10;
}