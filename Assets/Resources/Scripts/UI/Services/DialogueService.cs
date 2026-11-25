using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сервис диалогов. Также владеет памятью об NPC (секция "npcMemory"
/// в общем сохранении) — пока хранит только эмоции и последнюю реплику.
/// </summary>
public class DialogueService : BaseService, IDialogueService, ISaveModule
{
    private readonly Dictionary<string, NpcMemoryEntry> _npcMemories = new Dictionary<string, NpcMemoryEntry>();

    protected override Type GetServiceType() => typeof(IDialogueService);

    protected override void Awake()
    {
        base.Awake();
        ServiceLocator.Instance.GetService<ISaveService>()?.RegisterModule(this);
    }

    public DialogueData GetDialogue(string dialogueId)
    {
        try
        {
            var resourceService = ServiceLocator.Instance.GetService<IResourceService>();
            if (resourceService == null)
            {
                Debug.LogError("ResourceService not available");
                return null;
            }
            var data = resourceService.LoadJson<DialogueData>($"Data/DialogData/Dialog/{dialogueId}");
            data?.LoadPortrait();

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load dialogue {dialogueId}: {e.Message}");
            return null;
        }
    }

    public AIDialogueData GetAIDialogue(string npcId)
    {
        try
        {
            var resourceService = ServiceLocator.Instance.GetService<IResourceService>();
            if (resourceService == null)
            {
                Debug.LogError("ResourceService not available");
                return null;
            }
            var data = resourceService.LoadJson<AIDialogueData>($"Data/DialogData/AIDialog/{npcId}");
            // ��������� ������� ����� ��������������
            data?.LoadPortrait();

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load AI dialogue for {npcId}: {e.Message}");
            return null;
        }
    }

    public void ExecuteDialogueAction(DialogueAction action)
    {
        Debug.Log($"Executing dialogue action: {action.type} - {action.actionId}");
        // ����� ����� ������ ���������� �������� (������ ���������, ����� ������� � �.�.)
    }

    public bool CheckCondition(DialogueCondition condition)
    {
        // ��������� ���������� - ������ ���������� true
        // � ������� ����� ����� �������� ������� (�������� �������, ������� ��������� � �.�.)
        return true;
    }

    // ===== Память NPC (ISaveModule, секция "npcMemory") =====

    public string Key => "npcMemory";

    public NpcMemoryEntry GetMemory(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return null;
        return _npcMemories.TryGetValue(npcId, out var entry) ? entry : null;
    }

    public void SaveExchange(string npcId, string playerMessage, string npcReply, float[] emotions)
    {
        if (string.IsNullOrEmpty(npcId)) return;

        if (!_npcMemories.TryGetValue(npcId, out var entry))
        {
            entry = new NpcMemoryEntry { npcId = npcId };
            _npcMemories[npcId] = entry;
        }

        entry.lastPlayerMessage = playerMessage;
        entry.lastNpcReply = npcReply;
        entry.interactionCount++;
        if (emotions != null && emotions.Length == 8)
            entry.emotions = (float[])emotions.Clone();

        ServiceLocator.Instance.GetService<ISaveService>()?.RequestSave();
    }

    public string CaptureStateJson()
    {
        var state = new NpcMemoryState();
        foreach (var kv in _npcMemories)
            state.entries.Add(kv.Value);
        return JsonUtility.ToJson(state);
    }

    public void RestoreStateJson(string json)
    {
        _npcMemories.Clear();
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var state = JsonUtility.FromJson<NpcMemoryState>(json);
            if (state?.entries == null) return;

            foreach (var entry in state.entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.npcId))
                    _npcMemories[entry.npcId] = entry;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DialogueService] Ошибка восстановления памяти NPC: {e.Message}");
        }
    }
}
