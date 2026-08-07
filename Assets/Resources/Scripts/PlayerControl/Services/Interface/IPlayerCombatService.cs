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

    /// <summary>Устанавливает состояние блока. Вызывается из PlayerStateMachine при нажатии/отпускании кнопки блока.</summary>
    void SetBlocking(bool isBlocking);

    /// <summary>
    /// Наносит урон игроку с учётом активного блока.
    /// </summary>
    /// <param name="damage">Базовый урон.</param>
    /// <param name="source">Источник урона (может быть null).</param>
    void ApplyDamage(int damage, GameObject source = null);
}
