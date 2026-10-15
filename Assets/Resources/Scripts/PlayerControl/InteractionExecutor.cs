using UnityEngine;

public class InteractionExecutor : MonoBehaviour
{
    private IInteractionTargetingService _targeting;

    private void Start()
    {
        _targeting = ServiceLocator.Instance.GetService<IInteractionTargetingService>();
        var input = ServiceLocator.Instance.GetService<IInputService>();
        if (input != null)
        {
            input.OnInteract += ExecuteInteract;
        }
    }

    private void OnDestroy()
    {
        var input = ServiceLocator.Instance.GetService<IInputService>();
        if (input != null)
        {
            input.OnInteract -= ExecuteInteract;
        }
    }

    private void ExecuteInteract()
    {
        var target = _targeting.CurrentTarget;
        if (target == null) return;

        target.Interact(gameObject, "pickup");
    }
}
