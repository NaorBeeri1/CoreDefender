using UnityEngine;

[CreateAssetMenu(fileName = "NewBombData", menuName = "CoreDefender/Bomb Data")]
public class BombData : ScriptableObject
{
    [Header("Bomb Identity")]
    public string bombName = "Tactical Bomb";
    public int cost = 50;

    [Header("Combat Stats")]
    public int damage = 100;
    public float detonationRange = 2f;
}