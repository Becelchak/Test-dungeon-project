using EventBusSystem;
using UnityEngine;

namespace SceneLoad
{
    public interface ISceneLoadStartedSubscriber : IGlobalSubscriber
    {
        void OnSceneLoadStarted(string sceneName);
    }

    public interface ISceneLoadProgressSubscriber : IGlobalSubscriber
    {
        void OnSceneLoadProgress(float progress);
    }

    public interface ISceneLoadCompletedSubscriber : IGlobalSubscriber
    {
        void OnSceneLoadCompleted(string sceneName);
    }
}
