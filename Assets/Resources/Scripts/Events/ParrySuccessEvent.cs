using EventBusSystem;
using UnityEngine;

public class ParrySuccessEvent
{
    public GameObject sourceParry { get; }
    public GameObject targetParry { get; }

    public ParrySuccessEvent (GameObject sourceParry, GameObject targetParry)
    {
        this.sourceParry = sourceParry;
        this.targetParry = targetParry;
    }

}

public interface IParryEventSubscriber : IGlobalSubscriber
{
    void OnParryEvent(ParrySuccessEvent parryEvent);
}
