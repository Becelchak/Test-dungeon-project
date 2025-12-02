using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ClassicalDialogueView : BaseView<ClassicalDialogueViewModel>
{
    [Header("Classical Dialogue UI")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform responsesContainer;
    [SerializeField] private GameObject responseButtonPrefab;

    protected override void SetupBindings()
    {
        // Привязка имени NPC
        npcNameText.text = ViewModel.NpcName;

        // Подписка на изменения свойств
        ViewModel.PropertyChanged += OnPropertyChanged;

        // Инициализация начальных значений
        dialogueText.text = ViewModel.DialogueText;
        UpdateResponseButtons();
    }

    protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.DialogueText):
                dialogueText.text = ViewModel.DialogueText;
                break;

            case nameof(ViewModel.Responses):
                UpdateResponseButtons();
                break;

            case nameof(ViewModel.NpcName):
                npcNameText.text = ViewModel.NpcName;
                break;
        }
    }

    private void UpdateResponseButtons()
    {
        // Очистка старых кнопок
        foreach (Transform child in responsesContainer)
        {
            Destroy(child.gameObject);
        }

        // Создание новых кнопок для каждого ответа
        if (ViewModel.Responses != null)
        {
            foreach (var response in ViewModel.Responses)
            {
                var buttonObj = Instantiate(responseButtonPrefab, responsesContainer);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                text.text = response.Text;

                // Привязка команды с параметром ResponseId
                button.onClick.AddListener(() =>
                {
                    if (response.SelectCommand != null && response.SelectCommand.CanExecute(response.ResponseId))
                    {
                        response.SelectCommand.Execute(response.ResponseId);
                    }
                });
            }
        }
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