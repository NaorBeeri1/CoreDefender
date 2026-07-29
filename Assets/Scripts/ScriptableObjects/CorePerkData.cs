using UnityEngine;

[CreateAssetMenu(fileName = "NewCorePerk", menuName = "CoreDefender/Core Perk Data")]
public class CorePerkData : ScriptableObject
{
    [Header("Perk Identity")]
    public string perkName = "Overcharged Capacitors";
    [TextArea(2, 4)]
    public string perkDescription = "Increases all turret fire rates by 25%.";

    [Header("Stat Modifiers")]
    public float fireRateBonusMultiplier = 1.25f; // +25% fire rate
    public float creditBonusMultiplier = 1.2f;    // +20% bounty credits
    public int coreHealthBonus = 25;              // +25 Max Core HP
}