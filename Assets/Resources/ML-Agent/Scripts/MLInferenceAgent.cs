using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MLInferenceAgent : Agent
{
    [HideInInspector] public float[] currentEmotions = new float[8];
    [HideInInspector] public float wallMultiplier;
    [HideInInspector] public float lootMultiplier;
    [HideInInspector] public float trapMultiplier;
    [HideInInspector] public float goldMultiplier;
    [HideInInspector] public float respawnMultiplier;
    [HideInInspector] public float normalizedHealth;
    [HideInInspector] public float normalizedGold;
    [HideInInspector] public float normalizedProgress;

    public System.Action<float[]> OnActionsReceived; // действия

    public override void CollectObservations(VectorSensor sensor)
    {
        // 8 эмоций
        sensor.AddObservation(currentEmotions);
        // 5 множителей
        sensor.AddObservation(wallMultiplier);
        sensor.AddObservation(lootMultiplier);
        sensor.AddObservation(trapMultiplier);
        sensor.AddObservation(goldMultiplier);
        sensor.AddObservation(respawnMultiplier);
        // 3 состояния
        sensor.AddObservation(normalizedHealth);
        sensor.AddObservation(normalizedGold);
        sensor.AddObservation(normalizedProgress);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float[] actionsArray = new float[actions.ContinuousActions.Length];
        for (int i = 0; i < actions.ContinuousActions.Length; i++)
            actionsArray[i] = actions.ContinuousActions[i];
        Debug.Log($"Применяем действия: wall={actions.ContinuousActions[0]}, loot={actions.ContinuousActions[1]}, trap={actions.ContinuousActions[2]}, gold={actions.ContinuousActions[3]}, respawn={actions.ContinuousActions[4]}");
        OnActionsReceived?.Invoke(actionsArray);
    }
}