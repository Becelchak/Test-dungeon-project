using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;
using System.Text;

public class DungeonParameterAgent : Agent
{
    [Header("References")]
    [SerializeField] private DungeonSimulatorForTraining simulator;
    [SerializeField] private float[] currentEmotions = new float[8]; // 8 эмоций (Joy, Sadness, Anger, Fear, Surprise, Trust, Arousal, Dominance)

    [Header("Action Settings")]
    [SerializeField] private float actionScale = 0.1f; // сила изменения множителя за одно действие
    [SerializeField] private float minMultiplier = 0.5f;
    [SerializeField] private float maxMultiplier = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI reward;
    [SerializeField] private TextMeshProUGUI multiplers;
    [SerializeField] private TextMeshProUGUI emotion;

    private float episodeReward;
    [SerializeField] private float emotionChangeInterval = 10f; // секунды
    [SerializeField] private float emotionTimer = 10f;

    public override void Initialize()
    {
        // Ничего особенного
    }

    private void Update()
    {
        emotionTimer += Time.deltaTime;
        if (emotionTimer >= emotionChangeInterval)
        {
            RandomizeEmotions();
            emotionTimer = 0f;
        }
    }

    private void RandomizeEmotions()
    {
        var strBuild = new StringBuilder();
        for (int i = 0; i < currentEmotions.Length; i++)
        {
            var rndEmotion = Random.Range(0f, 1f);
            currentEmotions[i] = rndEmotion;
            switch (i)
            {
                case 0:
                    strBuild.Append($"Joy:{currentEmotions[i]} \\n");
                    break;
                case 1:
                    strBuild.Append($"Sadness:{currentEmotions[i]} \\n");
                    break;
                case 2:
                    strBuild.Append($"Anger:{currentEmotions[i]} \\n");
                    break;
                case 3:
                    strBuild.Append($"Fear:{currentEmotions[i]} \\n");
                    break;
                case 4:
                    strBuild.Append($"Surprise:{currentEmotions[i]}  \\n");
                    break;
                case 5:
                    strBuild.Append($"Trust:{currentEmotions[i]}  \\n");
                    break;
                case 6:
                    strBuild.Append($"Arousal:{currentEmotions[i]}  \\n");
                    break;
                case 7:
                    strBuild.Append($"Dominance:{currentEmotions[i]}  \\n");
                    break;
            }
        }
        emotion.text = strBuild.ToString();
    }

    public override void OnEpisodeBegin()
    {
        simulator.ResetSimulation();
        //RandomizeEmotions();
        // Сброс множителей до 1
        simulator.wallMultiplier = 1f;
        simulator.lootMultiplier = 1f;
        simulator.trapMultiplier = 1f;
        simulator.goldMultiplier = 1f;
        simulator.respawnMultiplier = 1f;
        episodeReward = 0f;

        reward.text = episodeReward.ToString();
        multiplers.text = $"wall:{simulator.wallMultiplier} \\n," +
            $"loot:{simulator.lootMultiplier} \\n," +
            $"trap:{simulator.trapMultiplier} \\n," +
            $"gold:{simulator.goldMultiplier} \\n," +
            $"respawn:{simulator.respawnMultiplier} \\n,";
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Эмоции (8)
        sensor.AddObservation(currentEmotions);
        // Текущие множители (5)
        sensor.AddObservation(simulator.wallMultiplier);
        sensor.AddObservation(simulator.lootMultiplier);
        sensor.AddObservation(simulator.trapMultiplier);
        sensor.AddObservation(simulator.goldMultiplier);
        sensor.AddObservation(simulator.respawnMultiplier);
        // Состояние игрока (3)
        sensor.AddObservation(simulator.playerHealth / 100f); // нормализовано
        sensor.AddObservation(simulator.playerGold / simulator.targetGold);
        sensor.AddObservation((float)simulator.currentStep / simulator.maxSteps);
        // Всего: 8+5+3 = 16
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Получаем непрерывные действия (5 значений в [-1,1])
        float enemyDelta = actions.ContinuousActions[0];
        float lootDelta = actions.ContinuousActions[1];
        float trapDelta = actions.ContinuousActions[2];
        float goldDelta = actions.ContinuousActions[3];
        float respawnDelta = actions.ContinuousActions[4];

        // Применяем изменения (мультипликативно)
        simulator.wallMultiplier = Mathf.Clamp(
            simulator.wallMultiplier * (1f + enemyDelta * actionScale),
            minMultiplier, maxMultiplier);
        simulator.lootMultiplier = Mathf.Clamp(
            simulator.lootMultiplier * (1f + lootDelta * actionScale),
            minMultiplier, maxMultiplier);
        simulator.trapMultiplier = Mathf.Clamp(
            simulator.trapMultiplier * (1f + trapDelta * actionScale),
            minMultiplier, maxMultiplier);
        simulator.goldMultiplier = Mathf.Clamp(
            simulator.goldMultiplier * (1f + goldDelta * actionScale),
            minMultiplier, maxMultiplier);
        simulator.respawnMultiplier = Mathf.Clamp(
            simulator.respawnMultiplier * (1f + respawnDelta * actionScale),
            minMultiplier, maxMultiplier);

        // Симулируем один шаг
        float stepReward = simulator.SimulateStep();
        AddReward(stepReward);
        episodeReward += stepReward;
        reward.text = episodeReward.ToString();

        multiplers.text = $"wall:{simulator.wallMultiplier} \\n," +
            $"loot:{simulator.lootMultiplier} \\n," +
            $"trap:{simulator.trapMultiplier} \\n," +
            $"gold:{simulator.goldMultiplier} \\n," +
            $"respawn:{simulator.respawnMultiplier} \\n,";

        // Если эпизод завершен, завершаем
        if (simulator.isEpisodeFinished)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Ручное управление для сбора демонстраций
        var continuousActions = actionsOut.ContinuousActions;
        // По умолчанию все нули (никаких изменений)
        continuousActions[0] = 0f;
        continuousActions[1] = 0f;
        continuousActions[2] = 0f;
        continuousActions[3] = 0f;
        continuousActions[4] = 0f;

        // Цифры 1-5 для увеличения соответствующего множителя
        if (Input.GetKey(KeyCode.Alpha1)) continuousActions[0] = 1f;
        if (Input.GetKey(KeyCode.Alpha2)) continuousActions[1] = 1f;
        if (Input.GetKey(KeyCode.Alpha3)) continuousActions[2] = 1f;
        if (Input.GetKey(KeyCode.Alpha4)) continuousActions[3] = 1f;
        if (Input.GetKey(KeyCode.Alpha5)) continuousActions[4] = 1f;

        // Shift + цифра для уменьшения
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKey(KeyCode.Alpha1)) continuousActions[0] = -1f;
            if (Input.GetKey(KeyCode.Alpha2)) continuousActions[1] = -1f;
            if (Input.GetKey(KeyCode.Alpha3)) continuousActions[2] = -1f;
            if (Input.GetKey(KeyCode.Alpha4)) continuousActions[3] = -1f;
            if (Input.GetKey(KeyCode.Alpha5)) continuousActions[4] = -1f;
        }

        Debug.Log($"Read input! {continuousActions[0]} {continuousActions[1]} {continuousActions[2]} {continuousActions[3]} {continuousActions[4]}");
    }
}