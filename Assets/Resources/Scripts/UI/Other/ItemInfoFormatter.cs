using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Форматирует ItemData в нейтральный InfoPanelContent для AdaptiveInfoPanel.
/// </summary>
public static class ItemInfoFormatter
{
    private static readonly Dictionary<ItemRarity, string> RarityNames = new()
    {
        { ItemRarity.Common, "Обычный" },
        { ItemRarity.Uncommon, "Необычный" },
        { ItemRarity.Rare, "Редкий" },
        { ItemRarity.Epic, "Эпический" },
        { ItemRarity.Legendary, "Легендарный" },
    };

    private static readonly Dictionary<ItemRarity, string> RarityColors = new()
    {
        { ItemRarity.Common, "B0B0B0" },
        { ItemRarity.Uncommon, "5CB85C" },
        { ItemRarity.Rare, "4A90D9" },
        { ItemRarity.Epic, "9B59D0" },
        { ItemRarity.Legendary, "F5A623" },
    };

    private static readonly Dictionary<ItemType, string> ItemTypeNames = new()
    {
        { ItemType.Weapon, "Оружие" },
        { ItemType.Trinket, "Безделушка" },
        { ItemType.Armor, "Броня" },
        { ItemType.Consumable, "Расходник" },
        { ItemType.Material, "Материал" },
    };

    private static readonly Dictionary<WeaponType, string> WeaponTypeNames = new()
    {
        { WeaponType.Unarmed, "Безоружный" },
        { WeaponType.Sword, "Меч" },
        { WeaponType.Axe, "Топор" },
        { WeaponType.Greatsword, "Двуручный меч" },
        { WeaponType.Dagger, "Кинжал" },
        { WeaponType.Shield, "Щит" },
        { WeaponType.Mace, "Булава" },
        { WeaponType.Polearm, "Древковое" },
    };

    public static string RarityName(ItemRarity rarity) =>
        RarityNames.TryGetValue(rarity, out var n) ? n : rarity.ToString();

    public static string RarityColorHex(ItemRarity rarity) =>
        RarityColors.TryGetValue(rarity, out var c) ? c : "FFFFFF";

    public static string ItemTypeName(ItemType type) =>
        ItemTypeNames.TryGetValue(type, out var n) ? n : type.ToString();

    /// <summary>Собирает контент инфо-панели для предмета.</summary>
    public static InfoPanelContent BuildContent(ItemData item)
    {
        var content = new InfoPanelContent();
        if (item == null) return content;

        content.Title = item.displayName;
        content.TitleColorHex = RarityColorHex(item.rarity);
        content.Description = item.description ?? string.Empty;
        content.StatLines = BuildStatLines(item);
        return content;
    }

    /// <summary>Строки характеристик для конкретного типа предмета.</summary>
    public static List<StatLine> BuildStatLines(ItemData item)
    {
        var lines = new List<StatLine>();
        if (item == null) return lines;

        if (item is WeaponData weapon)
        {
            BuildWeaponLines(weapon, lines);
        }
        else
        {
            lines.Add(new StatLine("Тип", ItemTypeName(item.itemType)));
            if (item.isStackable)
                lines.Add(new StatLine("Макс. стопка", item.maxStackSize.ToString(), true));
        }

        return lines;
    }

    private static void BuildWeaponLines(WeaponData weapon, List<StatLine> lines)
    {
        lines.Add(new StatLine("Тип", WeaponTypeNames.TryGetValue(weapon.weaponType, out var wt) ? wt : weapon.weaponType.ToString()));

        var stats = weapon.Stats;
        if (stats == null) return;

        if (weapon.weaponType == WeaponType.Shield)
        {
            lines.Add(new StatLine("Поглощение блока", Percent(stats.blockDamageReduction), true));
            lines.Add(new StatLine("Идеальный блок", Seconds(stats.perfectBlockWindow), true));
            lines.Add(new StatLine("Парирование", Seconds(stats.parryWindow), true));
            lines.Add(new StatLine("Стамина парирования", stats.parryStaminaCost.ToString("0.#"), true));
            return;
        }

        lines.Add(new StatLine("Урон", stats.damage.ToString("0.#"), true));
        if (stats.attackDuration > 0f)
            lines.Add(new StatLine("Скорость атаки", $"{(1f / stats.attackDuration):0.#}/с", true));
        if (stats.range > 0f)
            lines.Add(new StatLine("Дальность", stats.range.ToString("0.#"), true));
        lines.Add(new StatLine("Идеальный блок", Seconds(stats.perfectBlockWindow), true));
        lines.Add(new StatLine("Парирование", Seconds(stats.parryWindow), true));
        lines.Add(new StatLine("Поглощение блока", Percent(stats.blockDamageReduction), true));
        lines.Add(new StatLine("Стамина атаки", stats.attackStaminaCost.ToString("0.#"), true));
        lines.Add(new StatLine("Стамина парирования", stats.parryStaminaCost.ToString("0.#"), true));
    }

    private static string Percent(float v) => $"{v * 100f:0}%";
    private static string Seconds(float v) => $"{v:0.##} с";
}
