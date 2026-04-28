using System;
using UnityEngine;

public class LoadSceneAsync : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private ISceneLoadingService _sceneLoader;
    void Start()
    {
        _sceneLoader = ServiceLocator.Instance.GetService<ISceneLoadingService>();
        var windowsService = (WindowService) ServiceLocator.Instance.GetService<IWindowService>();
        windowsService.ShowLoadingScreen();
        _sceneLoader.LoadSceneAsync(sceneName, false);
    }

}
