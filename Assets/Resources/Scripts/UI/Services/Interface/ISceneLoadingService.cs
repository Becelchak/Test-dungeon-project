using Cysharp.Threading.Tasks;
using EventBusSystem;
using System;

public interface ISceneLoadingService : IGlobalSubscriber
{
    bool IsLoading { get; }
    UniTask LoadSceneAsync(string sceneName, bool needLoadLLM, Action<float> onProgress = null, Action onComplete = null);
    void CancelLoading();
}