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
    //[SerializeField] private Button clearLogButton;

    private List<GameObject> _instantiatedEntries = new List<GameObject>();

    protected override void SetupBindings()
    {
        //clearLogButton.onClick.AddListener(() => ViewModel.ClearLogCommand.Execute(null));
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
            if (entry != null)
                Destroy(entry);
        }
        _instantiatedEntries.Clear();

        // ВАЖНО: Создаем элементы в обратном порядке, чтобы новые были ВНИЗУ
        // Если хотите, чтобы новые были СВЕРХУ, используйте обычный foreach
        for (int i = ViewModel.LogEntries.Count - 1; i >= 0; i--)
        {
            var entryViewModel = ViewModel.LogEntries[i];
            var entryObj = Instantiate(logEntryPrefab, logEntriesContainer);

            // Если хотите, чтобы новые элементы добавлялись СВЕРХУ (первыми в иерархии)
            // entryObj.transform.SetAsFirstSibling();

            // Если хотите, чтобы новые добавлялись ВНИЗ (как сейчас, но в правильном порядке)
            // Оставьте как есть или используйте:
            entryObj.transform.SetSiblingIndex(0);

            var entryView = entryObj.GetComponent<LogEntryView>();

            if (entryView != null)
            {
                entryView.Bind(entryViewModel);
            }

            _instantiatedEntries.Add(entryObj);
        }

        // Прокручиваем к самому новому сообщению (вниз)
        ScrollToBottom();
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

        // Очищаем созданные объекты
        foreach (var entry in _instantiatedEntries)
        {
            if (entry != null)
                Destroy(entry);
        }
        _instantiatedEntries.Clear();

        base.Unbind();
    }
}