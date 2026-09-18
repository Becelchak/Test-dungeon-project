using System.Collections.Generic;
using UnityEngine;

public class SceneUILayerRegistry : MonoBehaviour
{
    [Header("Configure layers for this scene")]
    [SerializeField] private List<LayerContainer> sceneLayers;

    private void Start()
    {
        var windowService = ServiceLocator.Instance.GetService<IWindowService>() as WindowService;

        if (windowService != null)
        {
            windowService.RegisterSceneLayers(sceneLayers);
        }
        else
        {
            Debug.LogError("[SceneUILayerRegistry] Не удалось найти WindowService для регистрации слоев!");
        }
    }

    private void OnDestroy()
    {
        // Защита от утечек памяти
        var windowService = ServiceLocator.Instance.GetService<IWindowService>() as WindowService;
        windowService?.UnregisterSceneLayers();
    }
}
