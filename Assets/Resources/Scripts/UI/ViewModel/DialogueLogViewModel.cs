using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using UnityEngine;

public class DialogueLogViewModel : BaseViewModel
{
    private ObservableCollection<LogEntryViewModel> _logEntries = new ObservableCollection<LogEntryViewModel>();

    public ObservableCollection<LogEntryViewModel> LogEntries
    {
        get => _logEntries;
        private set => SetProperty(ref _logEntries, value);
    }

    public ICommand ClearLogCommand { get; }

    public DialogueLogViewModel()
    {
        ClearLogCommand = new RelayCommand(ClearLog);
    }

    public void AddEntry(string speakerName, Sprite portrait, string message, bool isPlayer = false)
    {
        Debug.Log($"Adding log entry: {speakerName} - {message} - Portrait: {portrait != null}");
        var entryData = new LogEntryData
        {
            speakerName = speakerName,
            speakerPortrait = portrait,
            messageText = message,
            isPlayer = isPlayer,
            timestamp = DateTime.Now
        };

        var entryViewModel = new LogEntryViewModel(entryData);

        // Добавляем в начало для "накопления вверх"
        LogEntries.Insert(0, entryViewModel);
        Debug.Log($"Total log entries: {LogEntries.Count}");
    }

    public void ClearLog()
    {
        LogEntries.Clear();
    }

    public override void Initialize()
    {

    }

    public override void Cleanup()
    {
        LogEntries.Clear();
    }
}