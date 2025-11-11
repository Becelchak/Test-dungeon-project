using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class DialogueView : BaseView<BaseViewModel>
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform responsesContainer;
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject responseButtonPrefab;
    [SerializeField] private GameObject loadingIndicator;

    [Header("View Configuration")]
    [SerializeField] private ClassicalDialogueView classicalView;
    [SerializeField] private AIDialogueView aiView;

    private MonoBehaviour _currentView;

    protected override void SetupBindings()
    {
        closeButton.onClick.AddListener(OnCloseClicked);

        // ќпредел€ет тип ViewModel и активирует соответствующий view
        if (ViewModel is ClassicalDialogueViewModel classicalVM)
        {
            classicalView.gameObject.SetActive(true);
            aiView.gameObject.SetActive(false);
            _currentView = classicalView;
            classicalView.Bind(classicalVM);
        }
        else if (ViewModel is AIDialogueViewModel aiVM)
        {
            classicalView.gameObject.SetActive(false);
            aiView.gameObject.SetActive(true);
            _currentView = aiView;
            aiView.Bind(aiVM);
        }
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {

    }

    private void OnCloseClicked()
    {
        ServiceLocator.Instance.GetService<IWindowService>().CloseWindow<BaseViewModel>();
    }

    public override void Unbind()
    {
        base.Unbind();
        if (_currentView is IView view)
        {
            view.Unbind();
        }
    }
}