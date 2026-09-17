using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueLogView : BaseView<DialogueLogViewModel>
{
    [Header("UI References")]
    [SerializeField] private Transform logEntriesContainer;
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollDuraction = 1.5f;
    //[SerializeField] private Button clearLogButton;

    private List<GameObject> _instantiatedEntries = new List<GameObject>();

    protected override void SetupBindings()
    {
        //clearLogButton.onClick.AddListener(() => ViewModel.ClearLogCommand.Execute(null));
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.LogEntries.CollectionChanged += OnLogEntriesChanged;
        for (var i = 0; i < gameObject.transform.childCount; i++) 
        {
            _instantiatedEntries.Add(gameObject.transform.GetChild(i).gameObject);
        }

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

        // Создаем элементы в обратном порядке, чтобы новые были ВНИЗУ
        for (int i = ViewModel.LogEntries.Count - 1; i >= 0; i--)
        {
            var entryViewModel = ViewModel.LogEntries[i];
            var entryObj = Instantiate(logEntryPrefab, logEntriesContainer);


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
            //StopCoroutine(SmoothScrollRectRutine());
            StartCoroutine(SmoothScrollRectRutine());
        }
    }

    /// <summary>
    /// Корутина для плавного пролистывания списка от текущего обьекта к последнему
    /// </summary>
    /// <returns></returns>
    private IEnumerator SmoothScrollRectRutine()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        float elapsed = 0f;
        float startPos = scrollRect.verticalNormalizedPosition;
        float targetPos = 0f;
        while(elapsed < scrollDuraction)
        {
            elapsed += Time.deltaTime;
            var timeTemp = elapsed / scrollDuraction;
            timeTemp = Mathf.SmoothStep(0f,1f,timeTemp);

            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, targetPos, timeTemp);
            yield return null;
        }
        scrollRect.verticalNormalizedPosition = targetPos;
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