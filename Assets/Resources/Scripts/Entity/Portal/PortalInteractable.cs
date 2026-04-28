using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class PortalInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneToLoad = "TestDungeon";
    [SerializeField] private float loadDelay = 0.5f;
    private bool canInteract = true;
    private SceneLoadingService sceneLoader;
    private WindowService windowsService;

    public void Start()
    {
        sceneLoader = (SceneLoadingService) ServiceLocator.Instance.GetService<ISceneLoadingService>();
        windowsService = (WindowService)ServiceLocator.Instance.GetService<IWindowService>();
        
    }

    public async void Interact(GameObject interactor)
    {
        ServiceLocator.Instance.GetService<IInputService>()?.DisableGameplayInput();
        canInteract = false;
        windowsService.ShowLoadingScreen();
        await sceneLoader.LoadSceneAsync(sceneToLoad, false);
    }

    //private IEnumerator LoadSceneAfterDelay()
    //{
    //    yield return new WaitForSeconds(loadDelay);
    //    SceneManager.LoadScene(sceneToLoad);
    //}

    public bool CanInteract(GameObject interactor) => canInteract;
    public string GetInteractionPrompt() => "[E] Войти в портал";
}