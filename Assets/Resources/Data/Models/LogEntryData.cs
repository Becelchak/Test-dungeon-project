using System;
using UnityEngine;

[System.Serializable]
public class LogEntryData
{
    public string speakerName;
    public Sprite speakerPortrait;
    public string messageText;
    public bool isPlayer;
    public DateTime timestamp;
}