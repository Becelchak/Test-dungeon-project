using System.Text;
using System.Linq;
using System;

public class PlayerContextService : BaseService, IPlayerContextService
{
    private PlayerProfileService _playerProfileService;

    protected override Type GetServiceType() => typeof(IPlayerContextService);

    void IPlayerContextService.Initialize()
    {
        _playerProfileService = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
    }

    public string GetPlayerContextForAI()
    {
        var profile = _playerProfileService.CurrentProfile;
        var sb = new StringBuilder();

        sb.AppendLine($"Игрок: {profile.playerName}");
        sb.AppendLine($"Степень прозрения: {profile.level} из 40 возможных");
        sb.AppendLine($"Жизненные силы: {profile.health}/{profile.maxHealth}");
        sb.AppendLine($"Магические силы: {profile.mana}/{profile.maxMana}");
        sb.AppendLine($"Характеристики: Стойкость({profile.strength} из 20 возможных), Разум({profile.intelligence} из 20 возможных), Эквилибристика({profile.agility} из 20 возможных)");

        sb.AppendLine("\nСодержимое сумки:");
        sb.AppendLine(GetPlayerInventorySummary());

        sb.AppendLine("\nСтатистика:");
        sb.AppendLine(GetPlayerStatsSummary());

        sb.AppendLine("\nАктивные поручения:");
        sb.AppendLine(GetActiveQuestsSummary());

        return sb.ToString();
    }

    public string GetPlayerInventorySummary()
    {
        var profile = _playerProfileService.CurrentProfile;
        if (profile.inventory.Count == 0)
            return "Инвентарь пуст";

        var sb = new StringBuilder();
        foreach (var item in profile.inventory.Take(10)) // Ограничиваем для избежания переполнения
        {
            sb.AppendLine($"- {item.itemName} x{item.quantity}");
        }

        if (profile.inventory.Count > 10)
            sb.AppendLine($"... и ещё {profile.inventory.Count - 10} предметов");

        return sb.ToString();
    }

    public string GetPlayerStatsSummary()
    {
        var stats = _playerProfileService.CurrentProfile.stats;
        return $"Убито врагов: {stats.enemiesKilled}\n" +
               $"Выполнено квестов: {stats.questsCompleted}\n" +
               $"Собрано золотых монет: {stats.goldCollected}\n" +
               $"Время в игре: {stats.playTimeHours:F1} часов {stats.playTimeHours:F2} минут";
    }

    public string GetActiveQuestsSummary()
    {
        var quests = _playerProfileService.CurrentProfile.quests;
        var activeQuests = quests.Where(q => q.Value.status == QuestStatus.InProgress).Take(5);

        if (!activeQuests.Any())
            return "Нет активных квестов";

        var sb = new StringBuilder();
        foreach (var quest in activeQuests)
        {
            sb.AppendLine($"- {quest.Key} (этап {quest.Value.currentStep})");
        }

        return sb.ToString();
    }
}