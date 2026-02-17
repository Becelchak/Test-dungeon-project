using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    private IInteractable _currentInteractable;

    public void Start()
    {
        var sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = interactionRadius;
    }
    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable.CanInteract(gameObject))
        {
            _currentInteractable = interactable;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == _currentInteractable)
        {
            _currentInteractable = null;
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