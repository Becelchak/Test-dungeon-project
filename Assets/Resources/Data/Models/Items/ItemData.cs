using UnityEngine;

public class ItemData : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Уникальный идентификатор предмета")]
    public string itemId;
    [Tooltip("Отображаемое имя предмета")]
    public string displayName;
    [Tooltip("Описание предмета")]
    [TextArea(3, 6)]
    public string description;
    [Tooltip("Иконка предмета")]
    public Sprite icon;
    [Tooltip("Тип предмета")]
    public ItemType itemType;
    [Tooltip("Редкость предмета")]
    public ItemRarity rarity;

    [Header("Стакирование")]
    [Tooltip("Можно ли складывать предмет в стопку")]
    public bool isStackable = false;
    [Tooltip("Максимальный размер стопки")]
    public int maxStackSize = 1;
}
