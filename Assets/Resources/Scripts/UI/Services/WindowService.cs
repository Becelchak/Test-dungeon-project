using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public struct LayerContainer
{
    public UILayer layer;
    public Transform containerTransform;
}
public class WindowService : BaseService, IWindowService
{
    [Header("Window Prefabs")]
    [SerializeField] private GameObject aiDialoguePrefab;
    [SerializeField] private GameObject classicalDialoguePrefab;
    [SerializeField] private GameObject inventoryGroupPrefab;
    [SerializeField] private GameObject loadingScreenPrefab;


    private Dictionary<Type, GameObject> _openWindows = new Dictionary<Type, GameObject>();
    private readonly Dictionary<UILayer, Transform> _cachedContainers = new();
    private Transform _windowsParent;

    protected override Type GetServiceType() => typeof(IWindowService);

    private void Start()
    {
        _windowsParent = GameObject.Find("DialogueWindows")?.transform;
        if (_windowsParent == null)
        {
            _windowsParent = GameObject.Find("Canvas UI")?.transform;
        }
    }

    public void RegisterSceneLayers(List<LayerContainer> newLayers)
    {
        _cachedContainers.Clear();

        foreach (var config in newLayers)
        {
            if (config.containerTransform != null)
            {
                _cachedContainers[config.layer] = config.containerTransform;
            }
        }

        Debug.Log($"[WindowService] Успешно зарегистрировано слоев для новой сцены: {_cachedContainers.Count}");
    }
    public void UnregisterSceneLayers()
    {
        _cachedContainers.Clear();
    }

    public GameObject ShowWindow<TViewModel>(UILayer layer, Action<TViewModel> onBeforeBind = null) where TViewModel : class, IViewModel
    {
        var vmType = typeof(TViewModel);
        if (_openWindows.ContainsKey(vmType))
        {
            Debug.LogWarning($"[WindowService] Окно с ViewModel {vmType.Name} уже открыто!");
            return _openWindows[vmType];
        }

        if (!_cachedContainers.TryGetValue(layer, out var parentTransform))
        {
            Debug.LogError($"[WindowService] Контейнер для слоя {layer} не назначен в инспекторе!");
            parentTransform = _windowsParent;
        }

        // Получение нужных префабов на основе запрашиваемой ViewModel
        GameObject prefab = GetPrefabByViewModelType(vmType);
        if (prefab == null) return null;

        // Спавн и бинд
        var windowObj = Instantiate(prefab, parentTransform);
        var view = windowObj.GetComponentInChildren<IView>(); // Предполагается общий интерфейс у BaseView

        // Создание инстанса ViewModel через фабрику
        // Если конструктор сложный, можно вынести снаружи в коллбэке onBeforeBind
        TViewModel viewModel = CreateViewModelInstance<TViewModel>();

        // Дается возможность внешней системе настроить ViewModel до биндинга (например, передать npcId)
        onBeforeBind?.Invoke(viewModel);

        viewModel.Initialize();

        if (view != null)
        {
            view.Bind(viewModel);
        }

        _openWindows[vmType] = windowObj;
        return windowObj;
    }

    private GameObject GetPrefabByViewModelType(Type type)
    {
        if (type == typeof(InventoryViewModel)) return inventoryGroupPrefab;
        if (type == typeof(AIDialogueViewModel)) return aiDialoguePrefab;
        if (type == typeof(ClassicalDialogueViewModel)) return classicalDialoguePrefab;
        if (type == typeof(LoadingScreenUI)) return loadingScreenPrefab; // условный пример

        Debug.LogError($"[WindowService] Не найден префаб для типа ViewModel: {type.Name}");
        return null;
    }

    private TViewModel CreateViewModelInstance<TViewModel>() where TViewModel : class, IViewModel
    {
        // Создаем экземпляр ViewModel (требуется пустой конструктор)
        return Activator.CreateInstance<TViewModel>();
    }

    public void CloseWindow<TViewModel>() where TViewModel : class, IViewModel
    {
        var type = typeof(TViewModel);
        if (_openWindows.TryGetValue(type, out var window))
        {
            var view = window.GetComponentInChildren<IView>();
            view?.Unbind();

            Destroy(window);
            _openWindows.Remove(type);
        }
    }

    public bool IsWindowOpen<TViewModel>() where TViewModel : class, IViewModel
    {
        return _openWindows.ContainsKey(typeof(TViewModel));
    }

    //public void ShowAIDialogue(string npcId)
    //{
    //    if (_openWindows.ContainsKey(typeof(AIDialogueViewModel)))
    //    {
    //        Debug.LogWarning("AI Dialogue window already open");
    //        return;
    //    }

    //    // Создаем общий лог для всех диалогов
    //    var logViewModel = new DialogueLogViewModel();
    //    var aiViewModel = new AIDialogueViewModel(npcId, logViewModel);

    //    var windowObj = Instantiate(AiDialogueWindowPrefab, _windowsParent);
    //    var view = windowObj.GetComponent<AIDialogueView>();

    //    if (view != null)
    //    {
    //        view.Bind(aiViewModel);
    //        _openWindows[typeof(AIDialogueViewModel)] = windowObj;
    //    }
    //}

    //public void ShowClassicalDialogue(string dialogueId)
    //{
    //    var logViewModel = new DialogueLogViewModel();
    //    var classicalViewModel = new ClassicalDialogueViewModel(dialogueId, logViewModel);

    //    if (_openWindows.ContainsKey(typeof(ClassicalDialogueViewModel)))
    //    {
    //        Debug.LogWarning("Classical Dialogue window already open");
    //        return;
    //    }

    //    if (ClassicalDialogueWindowPrefab == null)
    //    {
    //        Debug.LogError("Classical Dialogue Window Prefab not assigned!");
    //        return;
    //    }

    //    var windowObj = Instantiate(ClassicalDialogueWindowPrefab, _windowsParent);
    //    var view = windowObj.GetComponent<ClassicalDialogueView>();

    //    if (view != null)
    //    {
    //        view.Bind(classicalViewModel);
    //        _openWindows[typeof(ClassicalDialogueViewModel)] = windowObj;
    //    }
    //    else
    //    {
    //        Debug.LogError("ClassicalDialogueView component not found on prefab!");
    //        Destroy(windowObj);
    //    }
    //}

    //public void ShowLoadingScreen()
    //{
    //    if (_openWindows.ContainsKey(typeof(LoadingScreenUI)))
    //    {
    //        Debug.LogWarning("Loading Screen already open");
    //        return;
    //    }

    //    if (LoadScreenPrefab == null)
    //    {
    //        Debug.LogError("Loading Screen Prefab not assigned!");
    //        return;
    //    }

    //    var windowObj = Instantiate(LoadScreenPrefab, _windowsParent);
    //}

    //public void ShowInventoryGroup()
    //{
    //    if(InventoryGroup == null)
    //    {
    //        Debug.LogError("InventoryGroup Prefab not assigned!");
    //        return;
    //    }

    //    if (_openWindows.ContainsKey(typeof(InventoryViewModel)))
    //    {
    //        Debug.LogWarning("InventoryGroup already open");
    //        return;
    //    }

    //    var inventoryViewModel = new InventoryViewModel();
    //    var windowObj = Instantiate(InventoryGroup, _windowsParent);
    //    var view = windowObj.GetComponentInChildren<InventoryView>();

    //    if (view != null)
    //    {
    //        view.Bind(inventoryViewModel);
    //        _openWindows[typeof(InventoryViewModel)] = windowObj;
    //    }
    //    else
    //    {
    //        Debug.LogError("ClassicalDialogueView component not found on prefab!");
    //        Destroy(windowObj);
    //    }
    //}

    //public void CloseWindow<T>() where T : IViewModel
    //{
    //    if (_openWindows.TryGetValue(typeof(T), out var window))
    //    {
    //        Destroy(window);
    //        _openWindows.Remove(typeof(T));
    //    }
    //}

    //public bool IsWindowOpen<T>() where T : IViewModel
    //{
    //    return _openWindows.ContainsKey(typeof(T));
    //}
}