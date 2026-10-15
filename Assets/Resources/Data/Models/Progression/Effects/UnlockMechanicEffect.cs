using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Unlock Mechanic")]
public class UnlockMechanicEffect : SkillEffectData
{
    public string mechanicId;
    public override void Apply(ICharacterStatsCalculator c) => c.UnlockMechanic(mechanicId);
    public override void Remove(ICharacterStatsCalculator c) => c.LockMechanic(mechanicId);
}
