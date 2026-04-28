using EventBusSystem;
using UnityEngine;
using UnityEngine.UI;
using SceneLoad;
using System.Collections;
using Cysharp.Threading.Tasks;

public class LoadingScreenUI : MonoBehaviour, ISceneLoadStartedSubscriber, ISceneLoadProgressSubscriber, ISceneLoadCompletedSubscriber
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressImageSlider;
    private bool _useProgressBar = false;

    public void SetProgress(float p)
    {
        if (_useProgressBar && progressImageSlider != null)
            progressImageSlider.fillAmount = p;
    }

    private void OnEnable()
    {
        EventBus.Subscribe(this);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
    }

    private void OnDisable() => EventBus.Unsubscribe(this);

    public void OnSceneLoadStarted(string sceneName)
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        progressImageSlider.fillAmount = 0;
    }

    public void OnSceneLoadProgress(float progress)
    {
        progressImageSlider.fillAmount = Time.time * progress;
        Debug.Log($"FILL {progressImageSlider.fillAmount}");
    }

    public async void OnSceneLoadCompleted(string sceneName)
    {
        //StartCoroutine(LoadScreenFade());
        progressImageSlider.fillAmount = 1;
        await FadeOutAsync(0.97531f);
        if (this != null && gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }

    private async UniTask FadeOutAsync(float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            if (canvasGroup == null) return;

            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            await UniTask.Yield();
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
    }
}