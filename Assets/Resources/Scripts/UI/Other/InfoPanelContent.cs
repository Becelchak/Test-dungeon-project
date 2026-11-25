using System.Collections.Generic;
using System.Text;

/// <summary>
/// Одна строка «лейбл → значение» для колонок характеристик инфо-панели.
/// </summary>
public class StatLine
{
    public string Label;
    public string Value;
    /// <summary>Числовое значение — подсветить акцентным цветом.</summary>
    public bool HighlightValue;
    /// <summary>Переопределение цвета значения (html hex без #). Если null — берётся HighlightValue.</summary>
    public string ValueColorOverride;
    /// <summary>Переопределение цвета лейбла (html hex без #). Если null — стандартный.</summary>
    public string LabelColorOverride;

    public StatLine(string label, string value, bool highlightValue = false, string valueColorOverride = null, string labelColorOverride = null)
    {
        Label = label;
        Value = value;
        HighlightValue = highlightValue;
        ValueColorOverride = valueColorOverride;
        LabelColorOverride = labelColorOverride;
    }
}

/// <summary>
/// Нейтральное к типу данных содержимое инфо-панели.
/// Заполняется форматтером конкретного домена (ItemInfoFormatter, SkillInfoFormatter, ...).
/// BodySections[i] соответствует i-му текстовому полю sectionTexts панели.
/// </summary>
public class InfoPanelContent
{
    /// <summary>Заголовок (обычный текст, без rich-разметки).</summary>
    public string Title;
    /// <summary>Цвет заголовка (html hex без #). null — цвет по умолчанию.</summary>
    public string TitleColorHex;
    /// <summary>Основное описание (обычный текст или rich text). Пустой Title скрывает строку заголовка вместе с рамкой.</summary>
    public string Description;
    /// <summary>Дополнительные текстовые секции (лор, требования и т.п.) — уже с rich-разметкой.</summary>
    public List<string> BodySections = new List<string>();
    /// <summary>Строки характеристик — панель делит их на две колонки.</summary>
    public List<StatLine> StatLines = new List<StatLine>();
}

/// <summary>
/// Сборка TMP rich-text колонок из StatLine. Цвета подобраны под тёмный фон панелей.
/// </summary>
public static class StatLineFormatter
{
    public const string NumericColor = "FFD75E";    // золотистый для чисел
    public const string LabelColor = "C8C8C8";      // приглушённый для лейблов
    public const string PlainValueColor = "FFFFFF"; // обычный текст значения

    /// <summary>Собирает TMP-разметку для одной колонки характеристик.</summary>
    public static string BuildColumnText(IReadOnlyList<StatLine> lines)
    {
        if (lines == null || lines.Count == 0) return string.Empty;

        var sb = new StringBuilder(lines.Count * 32);
        foreach (var line in lines)
        {
            string valueColor = line.ValueColorOverride ?? (line.HighlightValue ? NumericColor : PlainValueColor);
            string labelColor = line.LabelColorOverride ?? LabelColor;
            sb.Append("<color=#").Append(labelColor).Append('>')
              .Append(line.Label)
              .Append("</color>  <color=#").Append(valueColor).Append('>')
              .Append(line.Value)
              .Append("</color>\n");
        }
        return sb.ToString();
    }
}
