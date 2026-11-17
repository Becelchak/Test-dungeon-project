using UnityEngine;

public struct EnemyKilledEvent
{
    public NpcData Data { get; }
    public GameObject Victim { get; }
    public GameObject Killer { get; }

    public EnemyKilledEvent(NpcData data, GameObject victim, GameObject killer)
    {
        Data = data;
        Victim = victim;
        Killer = killer;
    }
}
