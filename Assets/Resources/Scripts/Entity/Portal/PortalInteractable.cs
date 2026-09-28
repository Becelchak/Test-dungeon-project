using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class PortalInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneToLoad = "TestDungeon";
    [SerializeField] private float loadDelay = 0.5f;
    private bool canInteract = true;
    private SceneLoadingService sceneLoader;
    private IWindowService windowsService;

    public void Start()
    {
        sceneLoader = (SceneLoadingService) ServiceLocator.Instance.GetService<ISceneLoadingService>();
        windowsService = ServiceLocator.Instance.GetService<IWindowService>();
        
    }

    public async void Interact(GameObject interactor)
    {
        ServiceLocator.Instance.GetService<IInputService>()?.DisableAllInput();
        canInteract = false;
        windowsService.ShowWindow<DialogueLogViewModel>(UILayer.SystemMenu);
        await sceneLoader.LoadSceneAsync(sceneToLoad, false);
    }

    public bool CanInteract(GameObject interactor) => canInteract;
    public string GetInteractionPrompt() => "[E] Войти в портал";
}