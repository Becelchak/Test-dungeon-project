using EventBusSystem;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    private IInteractable _currentInteractable;


    public void Awake()
    {
        var sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = interactionRadius;
    }
    public void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable.CanInteract(gameObject))
        {
            _currentInteractable = interactable;
            //Debug.Log($"Interactor: вошёл в контакт с {other.name}, поднимаем событие");
            EventBus.RaiseEvent<IInteractionPromptEventSubscriber>(
            s => s.OnInteractionPrompt(new InteractionPromptEvent(interactable))
            );
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        var interactable = other.collider.GetComponent<IInteractable>();
        if (interactable != null && interactable.CanInteract(gameObject))
        {
            _currentInteractable = interactable;
            //Debug.Log($"Interactor: вошёл в контакт с {other.collider.name}, поднимаем событие");
            EventBus.RaiseEvent<IInteractionPromptEventSubscriber>(
            s => s.OnInteractionPrompt(new InteractionPromptEvent(interactable))
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == _currentInteractable)
        {
            _currentInteractable = null;
            EventBus.RaiseEvent<IInteractionPromptEventSubscriber>(
            s => s.OnInteractionPrompt(new InteractionPromptEvent(false))
            );
        }
    }

    public bool TryInteract()
    {
        if (_currentInteractable != null && _currentInteractable.CanInteract(gameObject))
        {
            _currentInteractable.Interact(gameObject);
            return true;
        }
        return false;
    }
}