using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerService : BaseService, IUIManagerService
{
    private struct OpenUiElement
    {
        public UILayer Layer;
        public Type ViewModelType;
        public Action CloseAction;
    }

    private IWindowService _windowService;
    private IInputService _input;
    private readonly Stack<OpenUiElement> _uiStack = new();

    protected override Type GetServiceType() => typeof(IUIManagerService);

    private void Start()
    {
        _windowService = ServiceLocator.Instance.GetService<IWindowService>();
        _input = ServiceLocator.Instance.GetService<IInputService>();

        _input.OnOpenInventory += ToggleInventory;
        _input.OnCancel += CloseTop;
    }

    public void ToggleInventory()
    {
        if (_windowService.IsWindowOpen<InventoryViewModel>())
        {
            CloseSpecificWindow<InventoryViewModel>();
            CloseSpecificWindow<EquipmentViewModel>();
            CloseSpecificWindow<ContextMenuViewModel>();
            //_input.SetGameplayInputActive(true);
        }
        else
        {
            // Открыть через WindowService, регистрируя операцию в стек UIManager
            _windowService.ShowWindow<InventoryViewModel>(UILayer.InventoryGroup);
            _windowService.ShowWindow<EquipmentViewModel>(UILayer.InventoryGroup);

            _uiStack.Push(new OpenUiElement
            {
                Layer = UILayer.InventoryGroup,
                ViewModelType = typeof(InventoryViewModel),
                CloseAction = () => _windowService.CloseWindow<InventoryViewModel>()
            });

            _uiStack.Push(new OpenUiElement
            {
                Layer = UILayer.InventoryGroup,
                ViewModelType = typeof(EquipmentViewModel),
                CloseAction = () => _windowService.CloseWindow<EquipmentViewModel>()
            });

            SetGameplayControlsEnabled(false);
        }
    }

    public void CloseTop()
    {
        if (_uiStack.Count == 0) return;

        var topUi = _uiStack.Pop();
        topUi.CloseAction?.Invoke();
        
        if (_uiStack.Count == 0)
        {
            SetGameplayControlsEnabled(true);
        }
    }

    private void CloseSpecificWindow<TViewModel>() where TViewModel : class, IViewModel
    {
        // Реализация быстрого закрытия конкретного окна,
        // даже если поверх инвентаря открыто что-то еще.
        // Пересборка стека без этого окна.
        var tempStack = new List<OpenUiElement>();
        bool found = false;

        while (_uiStack.Count > 0)
        {
            var element = _uiStack.Pop();
            if (element.ViewModelType == typeof(TViewModel))
            {
                element.CloseAction?.Invoke();
                found = true;
                break;
            }
            tempStack.Add(element);
        }

        // Возвращаем остальные элементы обратно в стек в правильном порядке
        for (int i = tempStack.Count - 1; i >= 0; i--)
        {
            _uiStack.Push(tempStack[i]);
        }

        if (_uiStack.Count == 0) SetGameplayControlsEnabled(true);
    }

    private void SetGameplayControlsEnabled(bool enabled)
    {
        _input.SetGameplayInputActive(enabled);
    }

    public void OpenWindow(UILayer layer)
    {
        throw new NotImplementedException();
    }

    public bool IsAnyUIOpen => _uiStack.Count > 0;
}

public enum UILayer
{
    None = 0,
    InventoryGroup = 10,
    GameMenu = 20,
    Settings = 21,
    Skills = 30,
    Dialogue = 50,
    ContextMenu = 60,
    SystemMenu = 100,
}
