using System;
using UnityEngine;

public interface ITriggerService
{
    void Register(TriggeredEffect trigger);
    void Unregister(TriggeredEffect trigger);

    void Dispatch(GameEvent evt, GameObject actor);

    void ResetEncounter();
    event Action<TriggeredEffect, GameContext> OnTriggerFired;
}