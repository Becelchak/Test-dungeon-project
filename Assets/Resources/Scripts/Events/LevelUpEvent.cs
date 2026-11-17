public struct LevelUpEvent
{
    public int NewLevel { get; }
    public int SkillPointsGranted { get; }

    public LevelUpEvent(int newLevel, int skillPointsGranted)
    {
        NewLevel = newLevel;
        SkillPointsGranted = skillPointsGranted;
    }
}
