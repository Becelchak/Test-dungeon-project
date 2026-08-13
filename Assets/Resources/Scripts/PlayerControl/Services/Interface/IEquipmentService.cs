using System;
using System.Collections.Generic;

public interface IEquipmentService
{
    /// <summary>
    /// Все слоты экипировки персонажа.
    /// </summary>
    IReadOnlyList<EquipmentSlot> Slots { get; }

    /// <summary>
    /// Индекс активного оружейного слота (0..2).
    /// </summary>
    int ActiveWeaponSlotIndex { get; }

    /// <summary>
    /// Текущее активное оружие из выбранного оружейного слота.
    /// </summary>
    WeaponData CurrentWeapon { get; }

    /// <summary>
    /// Текущий экипированный щит (OffHand) из любого оружейного слота.
    /// </summary>
    WeaponData CurrentShield { get; }

    /// <summary>
    /// Вызывается при изменении любого слота экипировки.
    /// </summary>
    event Action<EquipmentSlotType, ItemData> OnEquipmentChanged;

    /// <summary>
    /// Вызывается при смене активного оружия.
    /// </summary>
    event Action<WeaponData> OnWeaponChanged;

    /// <summary>
    /// Вызывается при смене/снятии щита.
    /// </summary>
    event Action<WeaponData> OnShieldChanged;

    /// <summary>
    /// Выбрать активный оружейный слот (0..2).
    /// </summary>
    void SetActiveWeaponSlot(int index);

    /// <summary>
    /// Экипировать предмет в указанный слот.
    /// </summary>
    bool Equip(ItemData item, EquipmentSlotType slotType);

    /// <summary>
    /// Снять предмет из указанного слота.
    /// </summary>
    bool Unequip(EquipmentSlotType slotType);

    /// <summary>
    /// Получить предмет в указанном слоте.
    /// </summary>
    ItemData GetItemInSlot(EquipmentSlotType slotType);

    /// <summary>
    /// Сериализовать текущую экипировку в JSON.
    /// </summary>
    string SaveToJson();

    /// <summary>
    /// Восстановить экипировку из JSON.
    /// </summary>
    void LoadFromJson(string json);
}
