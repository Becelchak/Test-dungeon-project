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

    public Transform Transform => transform;

    public void Start()
    {
        sceneLoader = (SceneLoadingService) ServiceLocator.Instance.GetService<ISceneLoadingService>();
        windowsService = ServiceLocator.Instance.GetService<IWindowService>();

    }

    public async void Interact(GameObject interactor, string optionId = "default")
    {
        ServiceLocator.Instance.GetService<IInputService>()?.DisableAllInput();
        canInteract = false;
        windowsService.ShowWindow<DialogueLogViewModel>(UILayer.SystemMenu);
        await sceneLoader.LoadSceneAsync(sceneToLoad, false);
    }

    public bool CanInteract(GameObject interactor) => canInteract;

    public string GetInteractionPrompt() => "[E] Войти в портал";

    public string GetDisplayName() => "Портал";

    public System.Collections.Generic.List<InteractionOption> GetOptions()
    {
        return new System.Collections.Generic.List<InteractionOption>
        {
            new InteractionOption { Id = "enter", Label = "Войти", Hotkey = KeyCode.E }
        };
    }
}
