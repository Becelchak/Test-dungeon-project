using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Одна строка технической характеристики предмета для инфо-панели.
/// </summary>
public class ItemStatLine
{
    public string Label;
    public string Value;
    /// <summary>Числовое значение — подсветить акцентным цветом.</summary>
    public bool HighlightValue;
    /// <summary>Переопределение цвета значения (html hex без #). Если null — берётся HighlightValue.</summary>
    public string ValueColorOverride;

    public ItemStatLine(string label, string value, bool highlightValue = false, string valueColorOverride = null)
    {
        Label = label;
        Value = value;
        HighlightValue = highlightValue;
        ValueColorOverride = valueColorOverride;
    }
}

/// <summary>
/// Форматирует технические характеристики ItemData для инфо-панели.
/// </summary>
public static class ItemInfoFormatter
{
    private const string NumericColor = "FFD75E";   // золотистый для чисел
    private const string LabelColor = "C8C8C8";     // приглушённый для лейблов
    private const string PlainValueColor = "FFFFFF";// обычный текст значения

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

    /// <summary>Название предмета, обёрнутое в цвет редкости (для TMP rich text).</summary>
    public static string FormatTitle(ItemData item)
    {
        if (item == null) return string.Empty;
        return $"<color=#{RarityColorHex(item.rarity)}>{item.displayName}</color>";
    }

    /// <summary>Строки характеристик для конкретного типа предмета.</summary>
    public static List<ItemStatLine> BuildStatLines(ItemData item)
    {
        var lines = new List<ItemStatLine>();
        if (item == null) return lines;

        lines.Add(new ItemStatLine("Редкость", RarityName(item.rarity), false, RarityColorHex(item.rarity)));

        if (item is WeaponData weapon)
        {
            BuildWeaponLines(weapon, lines);
        }
        else
        {
            lines.Add(new ItemStatLine("Тип", ItemTypeName(item.itemType)));
            if (item.isStackable)
                lines.Add(new ItemStatLine("Макс. стопка", item.maxStackSize.ToString(), true));
        }

        return lines;
    }

    private static void BuildWeaponLines(WeaponData weapon, List<ItemStatLine> lines)
    {
        lines.Add(new ItemStatLine("Тип", WeaponTypeNames.TryGetValue(weapon.weaponType, out var wt) ? wt : weapon.weaponType.ToString()));

        var stats = weapon.Stats;
        if (stats == null) return;

        if (weapon.weaponType == WeaponType.Shield)
        {
            lines.Add(new ItemStatLine("Поглощение блока", Percent(stats.blockDamageReduction), true));
            lines.Add(new ItemStatLine("Идеальный блок", Seconds(stats.perfectBlockWindow), true));
            lines.Add(new ItemStatLine("Парирование", Seconds(stats.parryWindow), true));
            lines.Add(new ItemStatLine("Стамина парирования", stats.parryStaminaCost.ToString("0.#"), true));
            return;
        }

        lines.Add(new ItemStatLine("Урон", stats.damage.ToString("0.#"), true));
        if (stats.attackDuration > 0f)
            lines.Add(new ItemStatLine("Скорость атаки", $"{(1f / stats.attackDuration):0.#}/с", true));
        if (stats.range > 0f)
            lines.Add(new ItemStatLine("Дальность", stats.range.ToString("0.#"), true));
        lines.Add(new ItemStatLine("Идеальный блок", Seconds(stats.perfectBlockWindow), true));
        lines.Add(new ItemStatLine("Парирование", Seconds(stats.parryWindow), true));
        lines.Add(new ItemStatLine("Поглощение блока", Percent(stats.blockDamageReduction), true));
        lines.Add(new ItemStatLine("Стамина атаки", stats.attackStaminaCost.ToString("0.#"), true));
        lines.Add(new ItemStatLine("Стамина парирования", stats.parryStaminaCost.ToString("0.#"), true));
    }

    /// <summary>Собирает TMP-разметку для одной колонки характеристик.</summary>
    public static string BuildColumnText(IReadOnlyList<ItemStatLine> lines)
    {
        if (lines == null || lines.Count == 0) return string.Empty;

        var sb = new StringBuilder(lines.Count * 32);
        foreach (var line in lines)
        {
            string valueColor = line.ValueColorOverride ?? (line.HighlightValue ? NumericColor : PlainValueColor);
            sb.Append("<color=#").Append(LabelColor).Append('>')
              .Append(line.Label)
              .Append("</color>  <color=#").Append(valueColor).Append('>')
              .Append(line.Value)
              .Append("</color>\n");
        }
        return sb.ToString();
    }

    private static string Percent(float v) => $"{v * 100f:0}%";
    private static string Seconds(float v) => $"{v:0.##} с";
}
