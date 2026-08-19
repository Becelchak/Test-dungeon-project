using Cysharp.Threading.Tasks;
using EventBusSystem;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneLoad;

public class SceneLoadingService : BaseService, ISceneLoadingService
{
    [Tooltip("Минимальное время демонстрации экрана загрузки")]
    [SerializeField] private double minDisplayTime;
    private AsyncOperation _currentOperation;
    public bool IsLoading => _currentOperation != null;
    private IAIService _aiService;

    protected override Type GetServiceType() => typeof(ISceneLoadingService);

    public async UniTask LoadSceneAsync(string sceneName, bool needLoadLLM, Action<float> onProgress = null, Action onComplete = null)
    {
        if (IsLoading) return;

        // Оповещаем UI
        EventBus.RaiseEvent<ISceneLoadStartedSubscriber>(s => s.OnSceneLoadStarted(sceneName));

        _currentOperation = SceneManager.LoadSceneAsync(sceneName);
        _currentOperation.allowSceneActivation = false;

        //var loadTask = UniTask.RunOnThreadPool(async () => {
        //    while (_currentOperation.progress < 0.9f)
        //    {
        //        float progress = Mathf.Clamp01(_currentOperation.progress / 0.9f);
        //        onProgress?.Invoke(progress);
        //        EventBus.RaiseEvent<ISceneLoadProgressSubscriber>(s => s.OnSceneLoadProgress(progress));
        //        await UniTask.Yield();
        //    }
        //    onProgress?.Invoke(1f);
        //    EventBus.RaiseEvent<ISceneLoadProgressSubscriber>(s => s.OnSceneLoadProgress(1f));
        //    _currentOperation.allowSceneActivation = true;
        //    await _currentOperation;
        //});

        //var loadTask = TrackProgressAsync(onProgress);

        var delayTask = UniTask.Delay(TimeSpan.FromSeconds(minDisplayTime));

        //_currentOperation.allowSceneActivation = true;
        //Debug.Log($"{_currentOperation.allowSceneActivation}");
        //await UniTask.WhenAll(loadTask, delayTask);

        //_currentOperation = null;
        //onComplete?.Invoke();
        //EventBus.RaiseEvent<ISceneLoadCompletedSubscriber>(s => s.OnSceneLoadCompleted(sceneName));

        //if (needLoadLLM)
        //{
        //    _aiService = ServiceLocator.Instance.GetService<IAIService>();
        //    if (_aiService != null)
        //    {
        //        var _loadingScreen = FindObjectOfType<LoadingScreenUI>(true);
        //        // Ждём загрузки модели
        //        await ((AIClient)_aiService).LoadModelAsync(new Progress<float>(p => {
        //            _loadingScreen?.SetProgress(p);
        //        }));
        //    }
        //}

        // 2. Ждем, пока прогресс дойдет до 0.9
        while (_currentOperation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(_currentOperation.progress / 0.9f);
            onProgress?.Invoke(progress);
            EventBus.RaiseEvent<ISceneLoadProgressSubscriber>(s => s.OnSceneLoadProgress(progress));
            await UniTask.Yield();
        }
        EventBus.RaiseEvent<ISceneLoadCompletedSubscriber>(s => s.OnSceneLoadCompleted(sceneName));

        _currentOperation.allowSceneActivation = true;

        // Ждем одновременно и таймер, и фактическое завершение загрузки (до 1.0)
        await UniTask.WhenAll(delayTask, _currentOperation.WithCancellation(this.GetCancellationTokenOnDestroy()));

        _currentOperation = null;
        onComplete?.Invoke();

    }

    //private async UniTask TrackProgressAsync(Action<float> onProgress)
    //{
    //    while (_currentOperation.progress < 0.9f)
    //    {
    //        float progress = Mathf.Clamp01(_currentOperation.progress / 0.9f);

    //        onProgress?.Invoke(progress);
    //        EventBus.RaiseEvent<ISceneLoadProgressSubscriber>(s => s.OnSceneLoadProgress(progress));

    //        await UniTask.Yield();
    //    }

    //    onProgress?.Invoke(1f);
    //    EventBus.RaiseEvent<ISceneLoadProgressSubscriber>(s => s.OnSceneLoadProgress(1f));

    //    //_currentOperation.allowSceneActivation = true;
    //    await _currentOperation;
    //}

    public void CancelLoading()
    {
        _currentOperation = null;
    }
}