using System.Collections.Generic;
using System;
using UnityEngine;

public class WindowService : IWindowService
{
    private readonly Dictionary<Type, GameObject> _openWindows = new();
    private readonly Dictionary<Type, GameObject> _windowPrefabs = new();

    public WindowService()
    {
        LoadWindowPrefabs();
    }

    private void LoadWindowPrefabs()
    {
        // Загрузка префабов окон из Resources
        _windowPrefabs[typeof(ClassicalDialogueViewModel)] = Resources.Load<GameObject>("UI/DialogueWindow");
        _windowPrefabs[typeof(AIDialogueViewModel)] = Resources.Load<GameObject>("UI/AIDialogueWindow");
    }

    public void ShowWindow<T>() where T : IViewModel
    {
        if (_openWindows.ContainsKey(typeof(T))) return;

        if (_windowPrefabs.TryGetValue(typeof(T), out var prefab))
        {
            var windowObj = UnityEngine.Object.Instantiate(prefab);
            var view = windowObj.GetComponent<IView>();

            // Добавить создание соответствующего ViewModel
            IViewModel viewModel = CreateViewModel<T>();
            view.Bind(viewModel);

            _openWindows[typeof(T)] = windowObj;
        }
    }

    private IViewModel CreateViewModel<T>()
    {
        // Фабричный метод для создания ViewModel

        return (IViewModel)Activator.CreateInstance<T>();
    }

    public void CloseWindow<T>() where T : IViewModel
    {
        if (_openWindows.TryGetValue(typeof(T), out var window))
        {
            UnityEngine.Object.Destroy(window);
            _openWindows.Remove(typeof(T));
        }
    }

    public bool IsWindowOpen<T>() where T : IViewModel
    {
        return _openWindows.ContainsKey(typeof(T));
    }
}