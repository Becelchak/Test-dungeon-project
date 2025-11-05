using System.Collections.Generic;
using System;
using UnityEngine;

public class WindowService : BaseService, IWindowService
{
    [Header("Window Prefabs")]
    public GameObject aiDialogueWindowPrefab;
    public GameObject classicalDialogueWindowPrefab;

    private Dictionary<Type, GameObject> _openWindows = new Dictionary<Type, GameObject>();
    private Transform _windowsParent;

    protected override Type GetServiceType() => typeof(IWindowService);

    private void Start()
    {
        _windowsParent = GameObject.Find("DialogueWindows")?.transform;
        if (_windowsParent == null)
        {
            // Создаем родителя если не существует
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var dialogueWindows = new GameObject("DialogueWindows");
                dialogueWindows.transform.SetParent(canvas.transform);
                _windowsParent = dialogueWindows.transform;
            }
        }
    }

    public void ShowAIDialogue(string npcId)
    {
        if (_openWindows.ContainsKey(typeof(AIDialogueViewModel)))
        {
            Debug.LogWarning("AI Dialogue window already open");
            return;
        }

        if (aiDialogueWindowPrefab == null)
        {
            Debug.LogError("AI Dialogue Window Prefab not assigned!");
            return;
        }

        var windowObj = Instantiate(aiDialogueWindowPrefab, _windowsParent);
        var view = windowObj.GetComponent<AIDialogueView>();

        if (view != null)
        {
            var viewModel = new AIDialogueViewModel(npcId);
            view.Bind(viewModel);
            _openWindows[typeof(AIDialogueViewModel)] = windowObj;
        }
        else
        {
            Debug.LogError("AIDialogueView component not found on prefab!");
            Destroy(windowObj);
        }
    }

    public void ShowClassicalDialogue(string dialogueId)
    {
        // Аналогичная реализация для классического диалога
        if (_openWindows.ContainsKey(typeof(ClassicalDialogueViewModel)))
        {
            Debug.LogWarning("Classical Dialogue window already open");
            return;
        }

        if (classicalDialogueWindowPrefab == null)
        {
            Debug.LogError("Classical Dialogue Window Prefab not assigned!");
            return;
        }

        var windowObj = Instantiate(classicalDialogueWindowPrefab, _windowsParent);
        var view = windowObj.GetComponent<ClassicalDialogueView>();

        if (view != null)
        {
            var viewModel = new ClassicalDialogueViewModel(dialogueId);
            view.Bind(viewModel);
            _openWindows[typeof(ClassicalDialogueViewModel)] = windowObj;
        }
        else
        {
            Debug.LogError("ClassicalDialogueView component not found on prefab!");
            Destroy(windowObj);
        }
    }

    public void CloseWindow<T>() where T : IViewModel
    {
        if (_openWindows.TryGetValue(typeof(T), out var window))
        {
            Destroy(window);
            _openWindows.Remove(typeof(T));
        }
    }

    public bool IsWindowOpen<T>() where T : IViewModel
    {
        return _openWindows.ContainsKey(typeof(T));
    }

    public void ShowWindow<T>() where T : IViewModel
    {
        throw new NotImplementedException("ShowWindow не прописано");
    }
}