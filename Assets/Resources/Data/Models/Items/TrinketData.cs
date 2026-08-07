using UnityEngine;

[CreateAssetMenu(fileName = "TrinketData", menuName = "Game/TrinketData")]
public class TrinketData : ItemData
{
    [Header("Характеристики (заглушка)")]
    [Tooltip("Бонусное здоровье")]
    public int bonusHealth;
}
