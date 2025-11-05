using System.Collections.Generic;
using System.ComponentModel.Design;
using System;
using UnityEngine;
public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ServiceLocator>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ServiceLocator");
                    _instance = obj.AddComponent<ServiceLocator>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Автоматически находит и регистрирует все сервисы на этом GameObject
        RegisterServicesOnThisObject();
    }

    private void RegisterServicesOnThisObject()
    {
        var services = GetComponents<BaseService>();
        foreach (var service in services)
        {
            // Сервисы зарегистрируют себя автоматически через BaseService.Awake()
            Debug.Log($"Found service: {service.GetType().Name}");
        }
    }

    public void RegisterService(Type serviceType, object service)
    {
        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"Service {serviceType.Name} already registered. Overwriting.");
            _services[serviceType] = service;
        }
        else
        {
            _services.Add(serviceType, service);
            Debug.Log($"Registered service: {serviceType.Name}");
        }
    }

    public void RegisterService<T>(T service) where T : class
    {
        RegisterService(typeof(T), service);
    }

    public T GetService<T>() where T : class
    {
        Type serviceType = typeof(T);
        if (_services.TryGetValue(serviceType, out var service))
        {
            return (T)service;
        }

        Debug.LogError($"Service {serviceType.Name} not found! Make sure it's registered in ServiceLocator.");
        return null;
    }

    public bool HasService<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }
}