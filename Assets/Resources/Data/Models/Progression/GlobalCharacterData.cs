using UnityEngine;

public enum SkillBranch
{
    warful = 1,
    polimorfizm = 2,
    pride = 3,
    godliness = 4,
    dichotomy = 5,
    scholarchip = 6,
    convict = 7,
    felon = 8,
    flagelliant = 9,
    scholar = 10,
    knight = 11
}

[System.Serializable]
public struct MinMaxInt
{
    public int min;
    public int max;
}

public enum GameEventCategory
{
    DamageTaken, DamageDealt, Kill, Death,
    ItemUsed, ItemEquipped, ItemPickedUp,
    InteractionStarted, InteractionCompleted,
    SkillActivated, StatChanged,
    DialogueStarted, DialogueEnded,
}

public abstract class GameEvent
{
    public abstract GameEventCategory Category { get; }
    public float Timestamp { get; } = Time.time;
}

public class DamageTakenEvent : GameEvent
{
    public override GameEventCategory Category => GameEventCategory.DamageTaken;
    public float Amount;
    public GameObject Attacker;
    public DamageType Type;
}

public class ItemUsedEvent : GameEvent
{
    public override GameEventCategory Category => GameEventCategory.ItemUsed;
    public ItemData Item;
    public bool Consumed;
}

public class KillEvent : GameEvent
{
    public override GameEventCategory Category => GameEventCategory.Kill;
    public GameObject Victim;
    [Tooltip("Кто был убит? Игрок или NPC?")]
    public bool WasPlayer;
}