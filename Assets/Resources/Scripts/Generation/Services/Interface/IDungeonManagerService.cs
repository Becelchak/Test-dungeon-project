using UnityEngine;

public interface IDungeonManagerService
{
    int RequiredGold { get; }
    bool IsLevelCompleted { get; }
}
