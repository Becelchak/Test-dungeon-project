using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDView : BaseView<PlayerHUDViewModel>
{
    [Header("Bars")]
    [SerializeField] private HudStatBar healthBar;
    [SerializeField] private HudStatBar manaBar;
    [SerializeField] private HudStatBar staminaBar;

    private void Awake()
    {
        if (ViewModel == null)
        {
            var vm = new PlayerHUDViewModel();
            vm.Initialize();
            Bind(vm);
        }
    }

    protected override void SetupBindings()
    {
        healthBar?.Initialize(ViewModel.Health, ViewModel.MaxHealth);
        manaBar?.Initialize(ViewModel.Mana, ViewModel.MaxMana);
        staminaBar?.Initialize(ViewModel.Stamina, ViewModel.MaxStamina);
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.Health):
            case nameof(ViewModel.MaxHealth):
                healthBar?.SetValue(ViewModel.Health, ViewModel.MaxHealth);
                break;

            case nameof(ViewModel.Mana):
            case nameof(ViewModel.MaxMana):
                manaBar?.SetValue(ViewModel.Mana, ViewModel.MaxMana);
                break;

            case nameof(ViewModel.Stamina):
            case nameof(ViewModel.MaxStamina):
                staminaBar?.SetValue(ViewModel.Stamina, ViewModel.MaxStamina);
                break;
        }
    }
}