using System.ComponentModel;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class AIDialogueView : BaseView<AIDialogueViewModel>
{
    [Header("AI Dialogue UI")]
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private ScrollRect scrollRect;

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    protected override void SetupBindings()
    {
        // Привязка поля ввода
        userInputField.onValueChanged.AddListener(ViewModel.SetInputText);
        ViewModel.InputText.BindTo(userInputField);

        // Привязка команды отправки
        sendButton.onClick.AddListener(() => ViewModel.SendMessageCommand.Execute());

        // Привязка индикатора загрузки
        ViewModel.IsLoading.BindTo(loadingIndicator.SetActive);

        // Привязка списка сообщений
        ViewModel.Messages.ObserveAdd().Subscribe(OnMessageAdded);
    }

    private void OnMessageAdded(CollectionAddEvent<MessageData> evt)
    {
        var messageObj = Instantiate(messagePrefab, chatContent);
        var messageView = messageObj.GetComponent<MessageView>();
        messageView.Bind(evt.Value);

        // Автопрокрутка к новому сообщению
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}