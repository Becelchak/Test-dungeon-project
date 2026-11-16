public struct StaminaChangedEvent
{
    public float CurrentStamina { get; }
    public float MaxStamina { get; }
    public StaminaChangedEvent(float current, float max)
    {
        CurrentStamina = current;
        MaxStamina = max;
    }
}