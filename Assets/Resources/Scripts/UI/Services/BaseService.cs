using System;
using UnityEngine;

public abstract class BaseService : MonoBehaviour
{
    protected virtual void Awake()
    {
        // Автоматическая регистрация в ServiceLocator
        var serviceType = GetServiceType();
        if (serviceType != null)
        {
            ServiceLocator.Instance.RegisterService(serviceType, this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (ServiceLocator.Instance != null && ServiceLocator.Instance != null)
        {
            var serviceType = GetServiceType();
            if (serviceType != null)
            {
                ServiceLocator.Instance.UnregisterService(serviceType);
            }
        }
    }

    protected abstract Type GetServiceType();
}