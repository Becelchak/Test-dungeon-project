using System.Collections.Generic;
using System.ComponentModel.Design;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static GameObject _prefab;
    [SerializeField] private static string PrefabServiceLocatorDefaultPath = "Prefabs/Other/ServiceLocator";
    //[SerializeField] private string defaultPrefabPath = "Prefabs/Other/ServiceLocator";

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null || _instance.Equals(null))
            {
                _instance = FindObjectOfType<ServiceLocator>();
                if (_instance == null && Application.isPlaying)
                {
                    if (_prefab == null)
                    {
                        _prefab = Resources.Load<GameObject>(PrefabServiceLocatorDefaultPath);
                    }
                    if (_prefab != null)
                    {
                        GameObject obj = Instantiate(_prefab);
                        _instance = obj.GetComponent<ServiceLocator>();
                        if (_instance == null)
                        {
                            Debug.LogError("Префаб ServiceLocator не содержит компонент ServiceLocator!");
                            Destroy(obj);
                            // Создаём пустой объект как запасной вариант
                            obj = new GameObject("ServiceLocator");
                            _instance = obj.AddComponent<ServiceLocator>();
                        }
                    }
                    else
                    {
                        GameObject obj = new GameObject("ServiceLocator");
                        _instance = obj.AddComponent<ServiceLocator>();
                    }

                    if (Application.isPlaying)
                        DontDestroyOnLoad(_instance.gameObject);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        Debug.Log("ЗАПУСТИЛСЯ ЛОКАТОР");
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        if (Application.isPlaying)
            DontDestroyOnLoad(_instance.gameObject);

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
            Debug.LogWarning($"Service {serviceType.Name} already registered. Owerwrite.");
            _services[serviceType] = service;
            return;
        }
        else
        {
            _services.Add(serviceType, service);
            Debug.Log($"Registered service: {serviceType.Name}");
        }
    }

    public void UnregisterService(Type serviceType)
    {
        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"Service {serviceType.Name} delete.");
            _services.Remove(serviceType);
        }
        else
        {
            Debug.Log($"Service: {serviceType.Name} not found for delete");
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

    public void OnDestroy()
    {
        Debug.Log("УНИЧТОЖИЛСЯ ЛОКАТОР");
    }

    public void DebugServices(string context)
    {
        Debug.Log($"=== ServiceLocator state at {context} ===");
        foreach (var kv in _services)
            Debug.Log($"- {kv.Key.Name} : {kv.Value}");
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DebugServices($"Scene loaded: {scene.name}");
        foreach (var kv in _services)
            Debug.Log($"- {kv.Key.Name} : {kv.Value.GetType().Name}");
    }
}