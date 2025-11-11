using System;
using UnityEngine;

public class LogEntryViewModel : BaseViewModel
{
    private string _speakerName;
    private Sprite _speakerPortrait;
    private string _messageText;
    private bool _isPlayer;
    private DateTime _timestamp;

    public string SpeakerName
    {
        get => _speakerName;
        set => SetProperty(ref _speakerName, value);
    }

    public Sprite SpeakerPortrait
    {
        get => _speakerPortrait;
        set => SetProperty(ref _speakerPortrait, value);
    }

    public string MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public bool IsPlayer
    {
        get => _isPlayer;
        set => SetProperty(ref _isPlayer, value);
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set => SetProperty(ref _timestamp, value);
    }

    public LogEntryViewModel(LogEntryData data)
    {
        SpeakerName = data.speakerName;
        SpeakerPortrait = data.speakerPortrait;
        MessageText = data.messageText;
        IsPlayer = data.isPlayer;
        Timestamp = data.timestamp;
    }

    public override void Initialize() { }
    public override void Cleanup() { }
}