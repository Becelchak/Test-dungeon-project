using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public string dialogueId;
    public string npcName;
    public string npcId;
    public string npcPortraitPath;
    [System.NonSerialized] public Sprite npcPortrait;
    public DialogueNode[] nodes;
    public string startNodeId;

    public void LoadPortrait()
    {
        if (!string.IsNullOrEmpty(npcPortraitPath))
        {
            npcPortrait = Resources.Load<Sprite>(npcPortraitPath);
            if (npcPortrait == null)
            {
                Debug.LogWarning($"Не удалось загрузить портрет по пути: {npcPortraitPath}");
            }
        }
    }
}

[System.Serializable]
public class DialogueNode
{
    public string nodeId;
    public string text;
    public DialogueCondition[] conditions;
    public DialogueAction[] actions;
    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseId;
    public string text;
    public string nextNodeId;
    public DialogueCondition[] conditions;
    public DialogueAction[] onSelected;
}

[System.Serializable]
public class DialogueCondition
{
    public string type; // "quest", "item", "flag"
    public string conditionId;
    public string value;
    public bool expectedResult = true;
}

[System.Serializable]
public class DialogueAction
{
    public string type; // "start_quest", "complete_quest", "give_item", "set_flag"
    public string actionId;
    public string value;
}

[System.Serializable]
public class AIDialogueData
{
    public string npcId;
    public string npcName;
    public string npcPortraitPath;
    [System.NonSerialized] public Sprite npcPortrait;
    public string initialPrompt;
    public AIDialogueConstraint[] constraints;
    public string personalityProfile;

    public void LoadPortrait()
    {
        if (!string.IsNullOrEmpty(npcPortraitPath))
        {
            npcPortrait = Resources.Load<Sprite>(npcPortraitPath);
            if (npcPortrait == null)
            {
                Debug.LogWarning($"Не удалось загрузить портрет по пути: {npcPortraitPath}");
            }
        }
    }
}

[System.Serializable]
public class AIDialogueConstraint
{
    public string type; // "topic_restriction", "response_length", "tone"
    public string constraint;
    public string value;
}

// Зачем?
[System.Serializable]
public class AICharacterData
{
    public string characterId;
    public string characterName;
    public string personality;
    public string background;
    public string communicationStyle;
    public string avatarPath;

    [System.NonSerialized]
    public Sprite avatar;
}

[System.Serializable]
public class PlayerProfile
{
    public string playerId = "player";
    public string playerName = "Рыцарь печального образа";
    public string avatarPath = "Sprites/Portraits/player_portrait";

    // Характеристики механические
    public int level;
    public int health;
    public int maxHealth;
    public int mana;
    public int maxMana;
    // Характеристики движения
    public float maxSpeed = 7f;
    public float speedMove = 5f;
    public float speedRun;
    public float acceleration = 15f;
    public float deceleration = 10f;
    public float rotationSpeed = 10f;
    // Характеристики ролевые
    public int strength;
    public int intelligence;
    public int agility;
    // Характеристики регенерации
    public float healthRegenRate;
    public float manahRegenRate;

    // Инвентарь
    public List<InventoryItem> inventory = new List<InventoryItem>();

    // Статистика
    public PlayerStats stats = new PlayerStats();

    // Прогресс квестов
    public Dictionary<string, QuestProgress> quests = new Dictionary<string, QuestProgress>();


    [System.NonSerialized]
    public Sprite avatar;
    public void LoadAvatar()
    {
        if (!string.IsNullOrEmpty(avatarPath))
        {
            avatar = Resources.Load<Sprite>(avatarPath);
            if (avatar == null)
            {
                Debug.LogWarning($"Не удалось загрузить портрет игрока по пути: {avatarPath}");
            }
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public string itemName;
    public string description;
    public int quantity = 1;
    public ItemType type;
    public Dictionary<string, int> attributes; // Дополнительные атрибуты
}

[System.Serializable]
public class PlayerStats
{
    public int enemiesKilled;
    public int questsCompleted;
    public int dialoguesCompleted;
    public int goldCollected;
    public float playTimeHours;
    public DateTime firstPlayDate;
}

[System.Serializable]
public class QuestProgress
{
    public string questId;
    public QuestStatus status;
    public int currentStep;
    public Dictionary<string, bool> objectives;
}

public enum ItemType { Weapon, Armor, Consumable, Quest, Miscellaneous }
public enum QuestStatus { NotStarted, InProgress, Completed, Failed }

[System.Serializable]
public class MessageData
{
    public string messageId;
    public string senderId; // "player" or characterId AI
    public string text;
    public DateTime timestamp;
    public bool isAI => senderId != "player";
}