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

    protected abstract Type GetServiceType();
}