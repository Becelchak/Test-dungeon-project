using System.Collections.Generic;
using System.Text;

/// <summary>
/// Форматирует SkillNodeViewModel в нейтральный InfoPanelContent для AdaptiveInfoPanel.
/// </summary>
public static class SkillInfoFormatter
{
    private const string UnlockedTitleColor = "FFD75E";  // золото — навык открыт
    private const string AvailableTitleColor = "FFB84D"; // оранжевый — можно открыть
    private const string LockedTitleColor = "8A8A8A";    // серый — заблокирован
    private const string UnlockedNameColor = "7CFC00";  // зелёный — требование выполнено
    private const string LockedNameColor = "FF5555";     // красный — требование не выполнено
    private const string SectionHeaderColor = "C8C8C8";

    /// <summary>Собирает контент инфо-панели для узла дерева навыков.</summary>
    public static InfoPanelContent Build(SkillNodeViewModel node)
    {
        var content = new InfoPanelContent();
        if (node == null) return content;

        content.Title = node.DisplayName;
        content.TitleColorHex = TitleColor(node);
        content.Description = node.Description ?? string.Empty;

        // Лор — отдельная секция, если задан
        if (!string.IsNullOrWhiteSpace(node.LoreText))
            content.BodySections.Add(node.LoreText);

        // Секция требований: предшественники (зелёные/красные) и черты
        string requirements = BuildRequirementsSection(node);
        if (!string.IsNullOrEmpty(requirements))
            content.BodySections.Add(requirements);

        // Строки характеристик
        content.StatLines.Add(new StatLine("Стоимость", node.Cost.ToString(), true));
        if (node.MinInsightLevel > 1)
            content.StatLines.Add(new StatLine("Требуется прозрение", node.MinInsightLevel.ToString(), true));

        return content;
    }

    private static string TitleColor(SkillNodeViewModel node)
    {
        if (node.IsUnlocked) return UnlockedTitleColor;
        if (node.CanUnlock) return AvailableTitleColor;
        return LockedTitleColor;
    }

    private static string BuildRequirementsSection(SkillNodeViewModel node)
    {
        var sb = new StringBuilder(256);
        bool hasContent = false;

        // Требуемые навыки-предшественники
        if (node.SkillNodes != null && node.SkillNodes.Count > 0)
        {
            AppendHeader(sb, "Требуемые навыки");
            hasContent = true;

            foreach (var required in node.SkillNodes)
            {
                if (required == null) continue;
                bool unlocked = node.IsNodeUnlocked(required.nodeId);
                sb.Append("<color=#").Append(unlocked ? UnlockedNameColor : LockedNameColor)
                  .Append(">• ").Append(required.displayName).Append("</color>\n");
            }
        }

        // Требуемые черты
        var traits = node.RequiredTraits;
        if (traits != null && traits.Count > 0)
        {
            if (hasContent) sb.Append('\n');
            AppendHeader(sb, "Требуемые черты");
            hasContent = true;

            foreach (var trait in traits)
            {
                sb.Append("<color=#").Append(SectionHeaderColor)
                  .Append(">• ").Append(trait).Append("</color>\n");
            }
        }

        return hasContent ? sb.ToString() : null;
    }

    private static void AppendHeader(StringBuilder sb, string header)
    {
        sb.Append("<color=#").Append(SectionHeaderColor).Append(">")
          .Append(header).Append("</color>\n");
    }
}
