using UnityEngine;

[CreateAssetMenu(fileName = "ArmorData", menuName = "Game/ArmorData")]
public class ArmorData : ItemData
{
    [Header("Тип брони")]
    [Tooltip("Слот брони, в который можно экипировать предмет")]
    public ArmorType armorType;

    [Header("Характеристики (заглушка)")]
    [Tooltip("Базовая защита")]
    public int armorValue;
}
