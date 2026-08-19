using UnityEngine;

/// <summary>
/// Сервис защиты NPC. Управляет блоком, снижением урона и окнами идеального блока.
/// </summary>
public class NpcDefenseService : MonoBehaviour
{
    public bool IsBlocking { get; private set; }
    public float BlockReduction { get; private set; }
    public float BlockStartTime { get; private set; }

    private float _blockEndTime;
    private float _nextBlockAllowedTime;
    private NpcData _data;

    public void Initialize(NpcData data)
    {
        _data = data;
        IsBlocking = false;
        BlockReduction = 0f;
    }

    /// <summary>
    /// Пытается начать блок. Возвращает true, если блок разрешён.
    /// </summary>
    public bool TryStartBlock()
    {
        if (_data == null) return false;
        if (IsBlocking) return true;
        if (Time.time < _nextBlockAllowedTime) return false;

        IsBlocking = true;
        BlockReduction = _data.blockDamageReduction;
        BlockStartTime = Time.time;
        _blockEndTime = Time.time + _data.blockDuration;
        return true;
    }

    /// <summary>
    /// Принудительно завершает блок и запускает кулдаун.
    /// </summary>
    public void EndBlock()
    {
        if (!IsBlocking) return;

        IsBlocking = false;
        BlockReduction = 0f;
        _nextBlockAllowedTime = Time.time + (_data?.blockCooldown ?? 1f);
    }

    /// <summary>
    /// Обновляет таймеры блока. Вызывается из состояния блока.
    /// </summary>
    public void TickBlock()
    {
        if (!IsBlocking) return;
        if (Time.time >= _blockEndTime)
            EndBlock();
    }

    /// <summary>
    /// Рассчитывает итоговый урон с учётом блока.
    /// </summary>
    public int ApplyBlock(int damage)
    {
        if (!IsBlocking || damage <= 0) return damage;

        float multiplier = 1f - Mathf.Clamp01(BlockReduction);
        return Mathf.RoundToInt(damage * multiplier);
    }
}
