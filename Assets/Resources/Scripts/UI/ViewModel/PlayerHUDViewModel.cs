using EventBusSystem;
public class PlayerHUDViewModel : BaseViewModel, 
    IHealthChangedEventSubscriber,
    IStaminaChangedEventSubscriber,
    IManaChangedEventSubscriber
{
    private IPlayerProfileService _player;

    private int _health;
    public int Health
    {
        get => _health;
        private set => SetProperty(ref _health, value);
    }

    private int _maxHealth;
    public int MaxHealth
    {
        get => _maxHealth;
        private set => SetProperty(ref _maxHealth, value);
    }

    private int _mana;
    public int Mana
    {
        get => _mana;
        private set => SetProperty(ref _mana, value);
    }

    private int _maxMana;
    public int MaxMana
    {
        get => _maxMana;
        private set => SetProperty(ref _maxMana, value);
    }

    private int _stamina;
    public int Stamina
    {
        get => _stamina;
        private set => SetProperty(ref _stamina, value);
    }

    private int _maxStamina;
    public int MaxStamina
    {
        get => _maxStamina;
        private set => SetProperty(ref _maxStamina, value);
    }

    public PlayerHUDViewModel()
    {

    }

    public void OnHealthChanged(HealthChangedEvent evt)
    {
        Health = evt.CurrentHealth;
        MaxHealth = evt.MaxHealth;
    }

    public void OnStaminaChanged(StaminaChangedEvent evt)
    {
        Stamina = evt.CurrentStamina;
        MaxStamina = evt.MaxStamina;
    }

    public void OnManaChanged(ManaChangedEvent evt)
    {
        Mana = evt.CurrentMana;
        MaxMana = evt.MaxMana;
    }

    public override void Initialize()
    {
        _player = ServiceLocator.Instance.GetService<IPlayerProfileService>();

        var p = _player.CurrentProfile;
        Health = p.health;
        MaxHealth = p.maxHealth;
        Mana = p.mana;
        MaxMana = p.maxMana;
        Stamina = p.stamina;
        MaxStamina = p.maxStamina;

        EventBus.Subscribe(this);
    }

    public override void Cleanup()
    {
        EventBus.Unsubscribe(this);
    }
}