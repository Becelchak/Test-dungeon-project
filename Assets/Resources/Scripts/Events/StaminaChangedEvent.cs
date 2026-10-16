public struct StaminaChangedEvent
{
    public int CurrentStamina { get; }
    public float MaxStamina { get; }
    public StaminaChangedEvent(int current, float max)
    {
        CurrentStamina = current;
        MaxStamina = max;
    }
}