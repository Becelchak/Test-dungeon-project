using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ContextMenuViewModel : BaseViewModel
{
    private List<IItemAction> _actions;
    private InventoryItemSlotViewModel _targetItem;
    private InventoryViewModel _inventoryViewModel;
    private bool _isVisible;

    public IReadOnlyList<IItemAction> Actions => _actions;
    public bool IsVisible
    {
        get => _isVisible;
        private set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    public event Action OnActionExecuted;

    public void Show(InventoryItemSlotViewModel itemVm, InventoryViewModel inventoryVm = null)
    {
        if (itemVm?.Data == null) return;

        _targetItem = itemVm;
        _inventoryViewModel = inventoryVm;
        _actions = ItemActionFactory.GetActionsForItem(itemVm.Data);
        IsVisible = true;
        OnPropertyChanged(nameof(Actions));
    }

    public void Hide()
    {
        IsVisible = false;
        _actions?.Clear();
        _targetItem = null;
        _inventoryViewModel = null;
    }

    public void ExecuteAction(int actionIndex)
    {
        if (_actions == null || actionIndex < 0 || actionIndex >= _actions.Count) return;
        if (_targetItem?.Data == null) return;

        var action = _actions[actionIndex];
        action.Execute(_targetItem.Data, _targetItem);

        // Обновляем инвентарь после выполнения действия
        _inventoryViewModel?.Reload();

        OnActionExecuted?.Invoke();
        Hide();
    }

    public override void Initialize()
    {
        // Инициализация не требуется
    }

    public override void Cleanup()
    {
        Hide();
    }
}
