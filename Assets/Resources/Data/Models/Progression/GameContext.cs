using System.Collections.Generic;
using UnityEngine;
public class GameContext
{
    public GameEvent Event { get; }
    [Tooltip("Кто инициатор")]
    public GameObject Actor { get; }
    [Tooltip("Отменено ли событие?")]
    public bool IsCancelled { get; set; }
    [Tooltip("Что в итоге применится")]
    public float FinalResult { get; set; }

    private readonly Dictionary<string, object> _bag = new();
    public void Set<T>(string key, T value) => _bag[key] = value;
    public bool TryGet<T>(string key, out T value)
    {
        if (_bag.TryGetValue(key, out var raw) && raw is T typed) { value = typed; return true; }
        value = default; return false;
    }

    public GameContext(GameEvent evt, GameObject actor)
    {
        Event = evt;
        Actor = actor;
    }
}