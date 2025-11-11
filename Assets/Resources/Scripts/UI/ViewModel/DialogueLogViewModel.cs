// ViewModels/DialogueLogViewModel.cs
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
    }

    public void ClearLog()
    {
        LogEntries.Clear();
    }

    public override void Initialize()
    {
        throw new NotImplementedException();
    }

    public override void Cleanup()
    {
        throw new NotImplementedException();
    }
}