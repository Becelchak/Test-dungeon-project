public interface IPlayerContextService
{
    void Initialize();
    string GetPlayerContextForAI();
    string GetPlayerInventorySummary();
    string GetPlayerStatsSummary();
    string GetActiveQuestsSummary();
}