using UnityEngine;

public class TriggerRuntime
{
    public float LastTriggerTime = -999f;
    public int UsesThisEncounter;
    public TriggeredEffect Source;

    public bool IsOnCooldown => Time.time - LastTriggerTime < Source.cooldown;
    public bool ExceededMaxUses => Source.maxPerEncounter > 0 && UsesThisEncounter >= Source.maxPerEncounter;

    public void MarkUsed() { LastTriggerTime = Time.time; UsesThisEncounter++; }
    public void ResetForEncounter() => UsesThisEncounter = 0;
}
