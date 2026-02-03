using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.ComponentModel;
using UnityEditor;
using System.Collections;

public class AIDialogueView : BaseView<AIDialogueViewModel>
{
    [Header("AI Dialogue UI")]
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Log")]
    [SerializeField] private DialogueLogView dialogueLogView;

    protected override void SetupBindings()
    {

        // Сначала устанавливаем значения по умолчанию
        npcNameText.text = "Загрузка...";
        dialogueText.text = "...";
        userInputField.text = "";
        sendButton.interactable = false;
        closeButton.interactable = true;

        // Подписка на изменения свойств
        ViewModel.PropertyChanged += OnPropertyChanged;

        // Привязка поля ввода - двусторонняя
        userInputField.onValueChanged.AddListener(value =>
        {
            if (ViewModel.UserInput != value)
            {
                ViewModel.UserInput = value;
            }
        });

        // Привязка команды отправки
        sendButton.onClick.AddListener(() => ViewModel.SendMessageCommand.Execute(null));
        closeButton.onClick.AddListener(() => ViewModel.CloseDialogueCommand.Execute(null));

        // Если ViewModel уже инициализирована, обновляем UI
        if (ViewModel.IsInitialized)
        {
            UpdateUI();
        }

        // Привязка DialogueLogView
        if (dialogueLogView != null && ViewModel.LogViewModel != null)
        {
            dialogueLogView.Bind(ViewModel.LogViewModel);
        }
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.IsInitialized):
                if (ViewModel.IsInitialized)
                {
                    UpdateUI();
                }
                break;

            case nameof(ViewModel.NpcName):
                npcNameText.text = ViewModel.NpcName ?? "Неизвестный NPC";
                break;

            case nameof(ViewModel.DialogueText):
                dialogueText.text = ViewModel.DialogueText ?? "...";
                break;

            case nameof(ViewModel.IsWaitingForResponse):
                loadingIndicator.SetActive(ViewModel.IsWaitingForResponse);
                UpdateSendButtonInteractable();
                break;

            case nameof(ViewModel.UserInput):
                if (userInputField.text != ViewModel.UserInput)
                {
                    userInputField.text = ViewModel.UserInput ?? "";
                }
                UpdateSendButtonInteractable();
                break;
        }
    }

    private void UpdateUI()
    {
        npcNameText.text = ViewModel.NpcName ?? "Неизвестный NPC";
        dialogueText.text = ViewModel.DialogueText ?? "...";
        userInputField.text = ViewModel.UserInput ?? "";
        UpdateSendButtonInteractable();
    }

    private void UpdateSendButtonInteractable()
    {
                sendButton.interactable = !ViewModel.IsWaitingForResponse && 
                                 !string.IsNullOrWhiteSpace(ViewModel.UserInput) &&
                                 ViewModel.IsInitialized;
    }

    protected override void OnDestroy()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }
        base.OnDestroy();
    }
}