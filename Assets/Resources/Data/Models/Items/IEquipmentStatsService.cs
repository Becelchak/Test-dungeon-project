using System;

public interface IEquipmentStatsService
{
    /// <summary>
    /// Текущие итоговые статы игрока (базовые + бонусы от экипировки).
    /// </summary>
    FinalPlayerStats CurrentStats { get; }

    /// <summary>
    /// Вызывается при любом изменении итоговых статов.
    /// </summary>
    event Action<FinalPlayerStats> OnStatsChanged;

    /// <summary>
    /// Принудительно пересчитать статы.
    /// </summary>
    void Recalculate();
}
