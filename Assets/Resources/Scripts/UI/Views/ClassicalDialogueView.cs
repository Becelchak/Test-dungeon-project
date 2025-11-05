using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassicalDialogueView : BaseView<ClassicalDialogueViewModel>
{
    [SerializeField] private Transform responsesContainer;
    [SerializeField] private GameObject responseButtonPrefab;

    private readonly List<GameObject> _responseButtons = new();

    protected override void SetupBindings()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        UpdateUI();
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.Responses):
                UpdateResponses();
                break;
            case nameof(ViewModel.DialogueText):
                UpdateDialogueText();
                break;
            case nameof(ViewModel.NpcName):
                UpdateNpcName();
                break;
        }
    }

    private void UpdateUI()
    {
        UpdateNpcName();
        UpdateDialogueText();
        UpdateResponses();
    }

    private void UpdateNpcName()
    {
        // npcNameText.text = ViewModel.NpcName;
    }

    private void UpdateDialogueText()
    {
        // dialogueText.text = ViewModel.DialogueText;
    }

    private void UpdateResponses()
    {
        // Удаление старых кнопок ответа
        foreach (var button in _responseButtons)
        {
            Destroy(button);
        }
        _responseButtons.Clear();

        // Создание новых кнопок ответа
        foreach (var response in ViewModel.Responses)
        {
            var buttonObj = Instantiate(responseButtonPrefab, responsesContainer);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            text.text = response.Text;
            button.onClick.AddListener(() => response.SelectCommand.Execute(response.ResponseId));

            _responseButtons.Add(buttonObj);
        }
    }
    public override void Unbind()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }
        base.Unbind();
    }
}