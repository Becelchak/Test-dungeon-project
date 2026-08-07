using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentSaveData
{
    /// <summary>
    /// Список сохранённых слотов экипировки.
    /// </summary>
    public List<SlotSaveData> slots = new List<SlotSaveData>();

    /// <summary>
    /// Индекс активного оружейного слота.
    /// </summary>
    public int activeWeaponSlotIndex;

    /// <summary>
    /// Индекс предыдущего активного оружейного слота.
    /// </summary>
    public int previousWeaponSlotIndex;
}

[Serializable]
public class SlotSaveData
{
    /// <summary>
    /// Тип слота экипировки.
    /// </summary>
    public EquipmentSlotType slotType;

    /// <summary>
    /// Уникальный идентификатор предмета в слоте. Пустая строка, если слот пуст.
    /// </summary>
    public string itemId;

    /// <summary>
    /// Разблокирован ли слот.
    /// </summary>
    public bool isUnlocked;
}
