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

    //public void OpenLayer(UILayer layer)
    //{
    //    if (IsAnyUIOpen) return;
    //    _windowService.OpenLayer(layer);
    //    _stack.Push(layer);
    //}

    public void ToggleInventory()
    {
        // Если инвентарь уже открыт — закрыть его
        if (_windowService.IsWindowOpen<InventoryViewModel>())
        {
            CloseSpecificWindow<InventoryViewModel>();
        }
        else
        {
            // Открыть через WindowService, регистрируя операцию в стек UIManager
            _windowService.ShowWindow<InventoryViewModel>(UILayer.InventoryGroup);

            _uiStack.Push(new OpenUiElement
            {
                Layer = UILayer.InventoryGroup,
                ViewModelType = typeof(InventoryViewModel),
                CloseAction = () => _windowService.CloseWindow<InventoryViewModel>()
            });

            SetGameplayControlsEnabled(false);
        }
    }
    //public void OpenDialogue(string npcId)
    //{
    //    _windowService.ShowWindow<AIDialogueViewModel>(UILayer.Dialogue, (vm) =>
    //    {
    //        // Сюда мы можем передать ID до того, как View забиндится. Надо ли?
    //    });

    //    _uiStack.Push(new OpenUiElement
    //    {
    //        Layer = UILayer.Dialogue,
    //        ViewModelType = typeof(AIDialogueViewModel),
    //        CloseAction = () => _windowService.CloseWindow<AIDialogueViewModel>()
    //    });

    //    SetGameplayControlsEnabled(false);
    //}

    public void CloseTop()
    {
        if (_uiStack.Count == 0) return;

        // Достаем верхнее окно из стека и уничтожаем его через его же зарегистрированный экшен
        var topUi = _uiStack.Pop();
        //if (topUi.Layer == UILayer.Dialogue)
        //{
        //    if(topUi.ViewModelType == typeof(AIDialogueViewModel))
        //        topUi.ViewModelType
        //}
        topUi.CloseAction?.Invoke();
        

        // Если окон больше нет — возвращаем управление персонажу
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
        // Логика блокировки контроллера игрока (мыши, перемещения), чтобы во время 
        // открытого инвентаря персонаж не бегал и не бил мечом.
        // Требует разделение ввода на UI и геймплей
        // _input.SetGameplayInputActive(enabled);
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
    SystemMenu = 100,
}
