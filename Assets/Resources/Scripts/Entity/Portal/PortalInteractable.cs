using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class PortalInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneToLoad = "TestDungeon";
    [SerializeField] private float loadDelay = 0.5f; // задержка для анимации
    private bool canInteract = true;

    public void Interact(GameObject interactor)
    {
        ServiceLocator.Instance.GetService<IInputService>()?.DisableGameplayInput();
        canInteract = false;
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneToLoad);
    }

    public bool CanInteract(GameObject interactor) => canInteract;
    public string GetInteractionPrompt() => "[E] Войти в портал";
}