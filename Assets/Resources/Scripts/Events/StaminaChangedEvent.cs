public struct StaminaChangedEvent
{
    public int CurrentStamina { get; }
    public int MaxStamina { get; }
    public StaminaChangedEvent(int current, int max)
    {
        CurrentStamina = current;
        MaxStamina = max;
    }
}