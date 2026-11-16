using EventBusSystem;
using Google.Protobuf;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
public class PlayerHUDViewModel : BaseViewModel,
    IHealthChangedEventSubscriber,
    IStaminaChangedEventSubscriber,
    IManaChangedEventSubscriber,
    IStatsChangedEventSubscriber
{
    private IPlayerProfileService _player;
    private ICharacterStatsCalculator _statsCalculator;

    private float _health;
    public int Health
    {
        get => Mathf.CeilToInt(_health);
        private set => SetProperty(ref _health, value);
    }

    private float _maxHealth;
    public int MaxHealth
    {
        get => Mathf.CeilToInt(_maxHealth);
        private set => SetProperty(ref _maxHealth, value);
    }

    private float _mana;
    public int Mana
    {
        get => Mathf.CeilToInt(_mana);
        private set => SetProperty(ref _mana, value);
    }

    private float _maxMana;
    public int MaxMana
    {
        get => Mathf.CeilToInt(_maxMana);
        private set => SetProperty(ref _maxMana, value);
    }

    private float _stamina;
    public int Stamina
    {
        get => Mathf.CeilToInt(_stamina);
        private set => SetProperty(ref _stamina, value);
    }

    private float _maxStamina;
    public int MaxStamina
    {
        get => Mathf.CeilToInt(_maxStamina);
        private set => SetProperty(ref _maxStamina, value);
    }

    public PlayerHUDViewModel()
    {

    }

    public void OnHealthChanged(HealthChangedEvent evt)
    {
        Health = Mathf.CeilToInt(evt.CurrentHealth);
        MaxHealth = Mathf.CeilToInt(evt.MaxHealth);
    }

    public void OnStaminaChanged(StaminaChangedEvent evt)
    {
        Stamina = Mathf.CeilToInt(evt.CurrentStamina);
        MaxStamina = Mathf.CeilToInt(evt.MaxStamina);
    }

    public void OnManaChanged(ManaChangedEvent evt)
    {
        Mana = Mathf.CeilToInt(evt.CurrentMana);
        MaxMana = Mathf.CeilToInt(evt.MaxMana);
    }

    public void OnStatsChanged(StatsChangedEvent evt)
    {
        MaxHealth = Mathf.CeilToInt(evt.Stats.MaxHealth);
        MaxMana = Mathf.CeilToInt(evt.Stats.MaxMana);
        MaxStamina = Mathf.CeilToInt(evt.Stats.MaxStamina);
    }

    public override void Initialize()
    {
        _player = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _statsCalculator = ServiceLocator.Instance.GetService<ICharacterStatsCalculator>();
        var p = _player.CurrentProfile;
        if (_statsCalculator == null)
        {
            Health = Mathf.CeilToInt(p.health);
            MaxHealth = p.maxHealth;
            Mana = Mathf.CeilToInt(p.mana);
            MaxMana = p.maxMana;
            Stamina = Mathf.CeilToInt(p.stamina);
            MaxStamina = p.maxStamina;
        }
        else
        {
            Health = Mathf.CeilToInt(p.health);
            MaxHealth = Mathf.CeilToInt(_statsCalculator.CurrentStats != null ? _statsCalculator.CurrentStats.MaxHealth : p.maxHealth);
            Mana = Mathf.CeilToInt(p.mana);
            MaxMana = Mathf.CeilToInt(_statsCalculator.CurrentStats != null ? _statsCalculator.CurrentStats.MaxMana : p.maxMana);
            Stamina = Mathf.CeilToInt(p.stamina);
            MaxStamina = Mathf.CeilToInt(_statsCalculator.CurrentStats != null ? _statsCalculator.CurrentStats.MaxStamina : p.stamina);
        }
        EventBus.Subscribe(this);

    }

    public override void Cleanup()
    {
        EventBus.Unsubscribe(this);
    }
}