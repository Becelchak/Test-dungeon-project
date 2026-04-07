using UnityEngine;

public class DungeonSimulatorForTraining : MonoBehaviour
{
    [Header("Base chances")]
    [SerializeField] private float baseEnemyChance = 0.1f;
    [SerializeField] private float baseLootChance = 0.1f;
    [SerializeField] private float baseTrapChance = 0.05f;

    [Header("Effects")]
    [SerializeField] private float goldPerLoot = 10f;
    [SerializeField] private float damagePerEnemy = 10f;
    [SerializeField] private float damagePerTrap = 5f;

    [Header("State")]
    public float playerHealth = 100f;
    public float playerGold = 0f;
    public float targetGold = 100f;
    public int currentStep = 0;
    public int maxSteps = 100;

    // Множители, которые будет изменять агент
    public float wallMultiplier { get; set; } = 1f;
    public float lootMultiplier { get; set; } = 1f;
    public float trapMultiplier { get; set; } = 1f;
    public float goldMultiplier { get; set; } = 1f;
    public float respawnMultiplier { get; set; } = 1f;

    public bool isEpisodeFinished { get; private set; }

    public void ResetSimulation()
    {
        playerHealth = 100f;
        playerGold = 0f;
        targetGold = Random.Range(50f, 250f) * goldMultiplier;
        currentStep = 0;
        isEpisodeFinished = false;
    }

    public float SimulateStep()
    {
        if (isEpisodeFinished) return 0f;

        float reward = 0f;
        currentStep++;

        // Враг
        if (Random.value < baseEnemyChance * wallMultiplier)
        {
            playerHealth -= damagePerEnemy;
            reward -= 0.1f; // штраф за урон
        }
        // Добыча
        if (Random.value < baseLootChance * lootMultiplier)
        {
            playerGold += goldPerLoot * goldMultiplier;
            reward += 0.05f; // поощрение за золото
        }
        // Ловушка
        if (Random.value < baseTrapChance * trapMultiplier)
        {
            playerHealth -= damagePerTrap;
            reward -= 0.05f;
        }

        playerHealth = Mathf.Clamp(playerHealth, 0, 100f);

        // Проверка окончания эпизода
        if (playerHealth <= 0)
        {
            isEpisodeFinished = true;
            reward -= 1f; // смерть
        }
        else if (playerGold >= targetGold)
        {
            isEpisodeFinished = true;
            reward += 1f; // победа
        }
        else if (currentStep >= maxSteps)
        {
            isEpisodeFinished = true;
            reward -= 0.5f; // не успел
        }

        return reward;
    }
}