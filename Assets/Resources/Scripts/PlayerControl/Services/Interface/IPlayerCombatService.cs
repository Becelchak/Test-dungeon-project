using UnityEngine;

/// <summary>
/// Сервис, отвечающий за бой игрока: получение урона, блок, идеальный блок/парирование.
/// </summary>
public interface IPlayerCombatService
{
    /// <summary>Возвращает true, если игрок сейчас удерживает блок.</summary>
    bool IsBlocking { get; }

    /// <summary>Время (Time.time), когда блок был активирован последний раз.</summary>
    float BlockStartTime { get; }

    /// <summary>Возвращает true, если сейчас активно окно парирования.</summary>
    bool IsParrying { get; }

    /// <summary>Время (Time.time), когда парирование было активировано.</summary>
    float ParryStartTime { get; }

    /// <summary>Текущее окно парирования в секундах.</summary>
    float ParryWindow { get; }

    /// <summary>Устанавливает состояние блока. Вызывается из PlayerStateMachine при нажатии/отпускании кнопки блока.</summary>
    void SetBlocking(bool isBlocking);

    /// <summary>
    /// Пытается начать парирование. Снимает стамину и открывает окно парирования.
    /// </summary>
    bool TryStartParry();

    /// <summary>
    /// Наносит урон игроку с учётом активного блока/парирования.
    /// </summary>
    /// <param name="damage">Базовый урон.</param>
    /// <param name="source">Источник урона (может быть null).</param>
    void ApplyDamage(int damage, GameObject source = null);
}
