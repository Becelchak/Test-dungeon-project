using System.Collections.Generic;
using System.ComponentModel.Design;
using System;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new();

    public static ServiceLocator Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeServices();
    }

    private void InitializeServices()
    {
        RegisterService<IDialogueService>(new DialogueService());
        RegisterService<IAIService>(FindObjectOfType<AIClient>());
        RegisterService<IResourceService>(new ResourceService());
        RegisterService<IWindowService>(new WindowService());
    }

    public void RegisterService<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public T GetService<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        throw new Exception($"Service {typeof(T)} not registered");
    }
}