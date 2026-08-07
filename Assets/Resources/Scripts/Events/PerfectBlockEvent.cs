using EventBusSystem;
using UnityEngine;

/// <summary>
/// Событие успешного идеального блока / парирования.
/// </summary>
public class PerfectBlockEvent
{
    /// <summary>Источник атаки (может быть null).</summary>
    public GameObject Source { get; }

    /// <summary>Оружие, которым был выполнен идеальный блок.</summary>
    public WeaponData Weapon { get; }

    public PerfectBlockEvent(GameObject source, WeaponData weapon)
    {
        Source = source;
        Weapon = weapon;
    }
}

public interface IPerfectBlockEventSubscriber : IGlobalSubscriber
{
    void OnPerfectBlock(PerfectBlockEvent evt);
}
