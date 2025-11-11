using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.UI;
using UnityEngine;

public class DialogueLogView : BaseView<DialogueLogViewModel>
{
    [Header("UI References")]
    [SerializeField] private Transform logEntriesContainer;
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button clearLogButton;

    private List<GameObject> _instantiatedEntries = new List<GameObject>();

    protected override void SetupBindings()
    {
        clearLogButton.onClick.AddListener(() => ViewModel.ClearLogCommand.Execute(null));
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.LogEntries.CollectionChanged += OnLogEntriesChanged;

        UpdateLogView();
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.LogEntries))
        {
            UpdateLogView();
        }
    }

    private void OnLogEntriesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateLogView();
        ScrollToBottom();
    }

    private void UpdateLogView()
    {
        // Очищаем старые элементы
        foreach (var entry in _instantiatedEntries)
        {
            Destroy(entry);
        }
        _instantiatedEntries.Clear();

        // Создаем новые элементы (в обратном порядке - новые внизу)
        for (int i = ViewModel.LogEntries.Count - 1; i >= 0; i--)
        {
            var entryViewModel = ViewModel.LogEntries[i];  // Теперь это ViewModel
            var entryObj = Instantiate(logEntryPrefab, logEntriesContainer);
            var entryView = entryObj.GetComponent<LogEntryView>();

            if (entryView != null)
            {
                entryView.Bind(entryViewModel);  // Передаем ViewModel, а не данные
            }

            _instantiatedEntries.Add(entryObj);
        }
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public override void Unbind()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
            ViewModel.LogEntries.CollectionChanged -= OnLogEntriesChanged;
        }
        base.Unbind();
    }
}