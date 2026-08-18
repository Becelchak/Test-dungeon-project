using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcStaggerState : NpcBaseState
{
    private double timer = 0f;
    public NpcStaggerState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();
        Machine.Combat.EndAttack();
        Machine.AnimationController.SetStagger(true);
        timer = CalculateStaggerTime();
    }

    public override void Update()
    {
        if (timer <= 0)
            Machine.TransitionToState(new NpcAttackState(Machine));
        timer -= Time.deltaTime;
    }
    
    public double CalculateStaggerTime()
    {
        return Random.Range(Data.minStaggerTime, Data.maxStaggerTime);
    }

    public override void Exit() 
    {
        Machine.AnimationController.SetStagger(false);
        base.Exit();
    }
}
