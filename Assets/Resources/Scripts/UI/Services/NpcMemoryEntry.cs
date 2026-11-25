using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Запись памяти об отдельном NPC: эмоции, последний обмен репликами.
/// Пока является заглушкой для будущей полноценной системы памяти NPC.
/// </summary>
[Serializable]
public class NpcMemoryEntry
{
    public string npcId;
    /// <summary>Последние эмоции NPC (joy, sadness, anger, fear, surprise, trust, arousal, dominance).</summary>
    public float[] emotions = new float[8];
    public string lastPlayerMessage;
    public string lastNpcReply;
    public int interactionCount;
}

/// <summary>Обёртка для JsonUtility (Dictionary он не сериализует).</summary>
[Serializable]
public class NpcMemoryState
{
    public List<NpcMemoryEntry> entries = new List<NpcMemoryEntry>();
}
