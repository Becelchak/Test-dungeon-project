using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgent : Agent
{
    public Transform Target;
    private Rigidbody rBody;

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        Debug.Log("Init");
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        Target.localPosition = new Vector3(3.5f, 1.5f, 3.4f);
        transform.position = new Vector3(0, 1.5f, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition.normalized);
        sensor.AddObservation(transform.localPosition.normalized);
        sensor.AddObservation(rBody.linearVelocity.normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Применяем действия
        Vector3 force = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        rBody.AddForce(force * 10);

        AddReward(-2.0f/MaxStep);

        // Награды
        float distance = Vector3.Distance(transform.localPosition, Target.localPosition);
        if (distance < 1.42f)
        {
            AddReward(1.0f);
            EndEpisode();
        }
    }
}